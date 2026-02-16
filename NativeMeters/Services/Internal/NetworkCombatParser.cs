using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Network;
using NativeMeters.Models.Internal;

namespace NativeMeters.Services.Internal;

internal enum ActorControlCategory : uint
{
    Death = 0x6,
    HoT = 0x604,
    DoT = 0x605,
}

public unsafe class NetworkCombatParser : IDisposable
{
    public event Action<ActionResultEvent>? OnActionResult;
    public event Action<ulong, string>? OnActorDeath;

    private const uint InvalidGameObjectId = 0xE0000000;
    private const uint MountActionOffset = 0xD000000;
    private const uint ItemActionOffset = 0x2000000;
    private const uint StrikingDummyNameId = 541;

    [Flags]
    private enum EffectFlags : byte
    {
        None = 0,
        Critical = 0x20,
        DirectHit = 0x40,
    }

    [Flags]
    private enum AmountFlags : byte
    {
        None = 0,
        HasHighBytes = 0x40,
    }

    private readonly StatusTracker statusTracker = new();

    private Hook<ActionEffectHandler.Delegates.Receive>? actionEffectHook;
    private Hook<PacketDispatcher.Delegates.HandleActorControlPacket>? actorControlHook;

    private bool enabled;

    public void Enable()
    {
        if (enabled) return;
        enabled = true;

        actionEffectHook = Service.GameInteropProvider.HookFromAddress(
            (nint)ActionEffectHandler.MemberFunctionPointers.Receive,
            new ActionEffectHandler.Delegates.Receive(ActionEffectDetour));

        actorControlHook = Service.GameInteropProvider.HookFromAddress(
            (nint)PacketDispatcher.MemberFunctionPointers.HandleActorControlPacket,
            new PacketDispatcher.Delegates.HandleActorControlPacket(ActorControlDetour));

        actionEffectHook.Enable();
        actorControlHook.Enable();

        Service.Logger.Information("[Internal Parser] Hooks enabled");
    }

    public void ResetTracking() => statusTracker.Clear();

    private void ActionEffectDetour(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos,
        ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        actionEffectHook!.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);

        try
        {
            if (header->NumTargets == 0) return;

            ulong resolvedSourceId = casterEntityId;
            string resolvedSourceName = casterPtr->NameString;
            uint resolvedSourceJobId = 0;

            if (casterPtr->GameObject.OwnerId != InvalidGameObjectId)
            {
                var owner = Service.ObjectTable.SearchById(casterPtr->GameObject.OwnerId);
                if (owner is IPlayerCharacter pcOwner)
                {
                    resolvedSourceId = pcOwner.GameObjectId;
                    resolvedSourceName = pcOwner.Name.TextValue;
                    resolvedSourceJobId = pcOwner.ClassJob.RowId;
                }
            }
            else if (Service.ObjectTable.SearchById(casterEntityId) is IPlayerCharacter pc)
            {
                resolvedSourceJobId = pc.ClassJob.RowId;
            }

            var actionId = (ActionType)header->ActionType switch
            {
                ActionType.Mount => MountActionOffset + header->ActionId,
                ActionType.Item => ItemActionOffset + header->ActionId,
                _ => header->SpellId
            };

            var isLimitBreak = false;
            if (actionId is > 0 and < MountActionOffset and < ItemActionOffset)
            {
                var action = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>()
                    .GetRowOrDefault(actionId);
                if (action.HasValue)
                {
                    isLimitBreak = action.Value.ActionCategory.RowId == 9;
                }
            }

            for (var i = 0; i < header->NumTargets; i++)
            {
                var targetId = (uint)(targetEntityIds[i] & uint.MaxValue);
                var targetObj = Service.ObjectTable.SearchById(targetId);
                if (targetObj == null) continue;

                uint currentHp = 0;
                uint maxHp = 0;

                if (targetObj is IBattleChara bc)
                {
                    currentHp = bc.CurrentHp;
                    maxHp = bc.MaxHp;
                }

                for (var j = 0; j < 8; j++)
                {
                    ref var effect = ref effects[i].Effects[j];
                    var type = (ActionEffectType)effect.Type;
                    if (type == ActionEffectType.Nothing) continue;

                    if (type != ActionEffectType.Damage &&
                        type != ActionEffectType.Heal &&
                        type != ActionEffectType.BlockedDamage &&
                        type != ActionEffectType.ParriedDamage &&
                        type != ActionEffectType.Miss) continue;

                    var amountFlags = (AmountFlags)effect.Param4;

                    uint amount = effect.Value;
                    if (amountFlags.HasFlag(AmountFlags.HasHighBytes))
                    {
                        amount += (uint)effect.Param3 << 16;
                    }

                    var evt = new ActionResultEvent
                    {
                        SourceId = resolvedSourceId,
                        SourceName = resolvedSourceName,
                        SourceJobId = resolvedSourceJobId,
                        TargetId = targetId,
                        TargetName = targetObj.Name.TextValue,
                        TargetCurrentHp = currentHp,
                        TargetMaxHp = maxHp,
                        TargetJobId = (targetObj is IPlayerCharacter tpc) ? tpc.ClassJob.RowId : 0,
                        IsPlayerTarget = targetObj is IPlayerCharacter,
                        ActionId = actionId,
                    };

                    switch (type)
                    {
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                            var damageFlags = (EffectFlags)effect.Param0;
                            OnActionResult?.Invoke(evt with {
                                Damage = amount,
                                IsCrit = damageFlags.HasFlag(EffectFlags.Critical),
                                IsDirectHit = damageFlags.HasFlag(EffectFlags.DirectHit),
                                IsLimitBreak = isLimitBreak,
                            });
                            break;

                        case ActionEffectType.Heal:
                            var healFlags = (EffectFlags)effect.Param1;
                            OnActionResult?.Invoke(evt with {
                                Healing = amount,
                                IsCrit = healFlags.HasFlag(EffectFlags.Critical),
                                IsLimitBreak = isLimitBreak,
                            });
                            break;

                        case ActionEffectType.Miss:
                            OnActionResult?.Invoke(evt with { Damage = 0, IsMiss = true, IsLimitBreak = isLimitBreak });
                            break;
                    }
                }
            }
        }
        catch (Exception ex) { Service.Logger.Error($"[Internal Parser] {ex}"); }
    }

    private void ActorControlDetour(
        uint entityId, uint category, uint arg1, uint arg2,
        uint arg3, uint arg4, uint arg5, uint arg6,
        uint arg7, uint arg8, GameObjectId targetId, bool isRecorded)
    {
        actorControlHook!.Original(entityId, category, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, targetId, isRecorded);

        try
        {
            switch ((ActorControlCategory)category)
            {
                case ActorControlCategory.DoT:
                    HandleDoTTick(entityId, arg1, arg2);
                    break;

                case ActorControlCategory.HoT:
                    HandleHoTTick(entityId, arg1, arg2);
                    break;

                case ActorControlCategory.Death:
                    if (Service.ObjectTable.SearchById(entityId) is IPlayerCharacter deadPc)
                        OnActorDeath?.Invoke(entityId, deadPc.Name.TextValue);
                    break;
            }
        }
        catch (Exception ex)
        {
            Service.Logger.Error($"[Internal Parser] ActorControl error: {ex.Message}");
        }
    }

    // DoT/HoT damage is estimated, I'm not going to decompile FFXIV Plugin to copy how they simulate DoTs.
    // If anyone wants to take a crack at it, be my guest but don't decompile Ravahn's FFXIV Plugin.
    private void HandleDoTTick(uint entityId, uint statusId, uint amount)
    {
        var target = Service.ObjectTable.SearchById(entityId);
        var localPlayer = Service.ObjectTable.LocalPlayer;
        if (localPlayer == null) return;

        if (statusId != 0)
        {
            var sourceId = statusTracker.GetSource(entityId, statusId);
            if (sourceId != null && Service.ObjectTable.SearchById((uint)sourceId) is IPlayerCharacter pc)
            {
                InvokeDoT(pc.GameObjectId, pc.Name.TextValue, pc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", amount);
                return;
            }
        }

        var sources = statusTracker.GetDoTSources(entityId);

        if (sources.Count == 0)
        {
            if (target is IBattleChara battle &&
                (battle.NameId == StrikingDummyNameId || battle.TargetObjectId == localPlayer.GameObjectId))
            {
                InvokeDoT(localPlayer.GameObjectId, localPlayer.Name.TextValue,
                           localPlayer.ClassJob.RowId, entityId,
                           target.Name.TextValue, amount);
            }
            return;
        }

        if (sources.Count == 1)
        {
            var sourceObj = Service.ObjectTable.SearchById((uint)sources[0]);
            if (sourceObj is IPlayerCharacter pc)
            {
                InvokeDoT(sources[0], pc.Name.TextValue, pc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", amount);
            }
            return;
        }

        var splitAmount = (uint)(amount / sources.Count);
        foreach (var sourceId in sources)
        {
            var sourceObj = Service.ObjectTable.SearchById((uint)sourceId);
            if (sourceObj is IPlayerCharacter pc)
            {
                InvokeDoT(sourceId, pc.Name.TextValue, pc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", splitAmount);
            }
        }
    }

    private void InvokeDoT(ulong sourceId, string sourceName, uint sourceJobId, uint targetId, string targetName, uint amount)
    {
        OnActionResult?.Invoke(new ActionResultEvent
        {
            SourceId = sourceId,
            SourceName = sourceName,
            SourceJobId = sourceJobId,
            TargetId = targetId,
            TargetName = targetName,
            IsPlayerTarget = false,
            Damage = amount,
            ActionId = 0,
        });
    }

    private void HandleHoTTick(uint entityId, uint statusId, uint amount)
    {
        if (Service.ObjectTable.SearchById(entityId) is not IPlayerCharacter pc) return;

        ulong resolvedSourceId = entityId;
        string resolvedSourceName = pc.Name.TextValue;
        uint resolvedSourceJobId = pc.ClassJob.RowId;

        if (statusId != 0)
        {
            var sourceId = statusTracker.GetSource(entityId, statusId);
            if (sourceId != null && Service.ObjectTable.SearchById(sourceId.Value) is IPlayerCharacter sourcePc)
            {
                resolvedSourceId = sourcePc.GameObjectId;
                resolvedSourceName = sourcePc.Name.TextValue;
                resolvedSourceJobId = sourcePc.ClassJob.RowId;
            }
        }

        OnActionResult?.Invoke(new ActionResultEvent
        {
            SourceId = resolvedSourceId,
            SourceName = resolvedSourceName,
            SourceJobId = resolvedSourceJobId,
            TargetId = entityId,
            TargetName = pc.Name.TextValue,
            TargetJobId = pc.ClassJob.RowId,
            IsPlayerTarget = true,
            Healing = amount,
        });
    }

    public void Dispose()
    {
        actionEffectHook?.Dispose();
        actorControlHook?.Dispose();
        statusTracker.Clear();
        enabled = false;
    }
}
