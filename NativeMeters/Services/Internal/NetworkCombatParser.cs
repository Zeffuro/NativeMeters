using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Network;
using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

internal enum ActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
}

internal enum ActorControlCategory : uint
{
    Death = 0x6,
    HoT = 0x604,
    DoT = 0x605,
}

public unsafe class NetworkCombatParser : IDisposable
{
    public event Action<ActionResultEvent>? OnActionResult;
    public event Action<uint, string>? OnActorDeath;

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
            ClassJob resolvedSourceJob = default;

            if (casterPtr->GameObject.OwnerId != 0xE0000000)
            {
                var owner = Service.ObjectTable.SearchById(casterPtr->GameObject.OwnerId);
                if (owner is IPlayerCharacter pcOwner)
                {
                    resolvedSourceId = pcOwner.GameObjectId;
                    resolvedSourceName = pcOwner.Name.TextValue;
                    resolvedSourceJob = Service.DataManager.GetExcelSheet<ClassJob>()
                        .GetRowOrDefault(pcOwner.ClassJob.RowId) ?? default;
                }
            }
            else if (Service.ObjectTable.SearchById(casterEntityId) is IPlayerCharacter pc)
            {
                resolvedSourceJob = Service.DataManager.GetExcelSheet<ClassJob>()
                    .GetRowOrDefault(pc.ClassJob.RowId) ?? default;
            }

            var actionId = (ActionType)header->ActionType switch
            {
                ActionType.Mount => 0xD000000u + header->ActionId,
                ActionType.Item => 0x2000000u + header->ActionId,
                _ => header->SpellId
            };

            for (var i = 0; i < header->NumTargets; i++)
            {
                var targetId = (uint)(targetEntityIds[i] & uint.MaxValue);
                var targetObj = Service.ObjectTable.SearchById(targetId);
                bool isPlayerTarget = targetObj is IPlayerCharacter;

                ClassJob targetJob = default;
                string targetName = targetObj?.Name.TextValue ?? "";
                if (targetObj is IPlayerCharacter tpc)
                {
                    targetJob = Service.DataManager.GetExcelSheet<ClassJob>()
                        .GetRowOrDefault(tpc.ClassJob.RowId) ?? default;
                }

                for (var j = 0; j < 8; j++)
                {
                    ref var effect = ref effects[i].Effects[j];
                    if (effect.Type == 0) continue;

                    uint amount = effect.Value;
                    if ((effect.Param4 & 0x40) == 0x40)
                        amount += (uint)effect.Param3 << 16;

                    var effectType = (ActionEffectType)effect.Type;

                    switch (effectType)
                    {
                        case ActionEffectType.Miss:
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                            OnActionResult?.Invoke(new ActionResultEvent
                            {
                                SourceId = resolvedSourceId,
                                SourceName = resolvedSourceName,
                                SourceJob = resolvedSourceJob,
                                TargetId = targetId,
                                TargetName = targetName,
                                TargetJob = targetJob,
                                IsPlayerTarget = isPlayerTarget,
                                Damage = effectType == ActionEffectType.Miss ? 0 : amount,
                                IsCrit = (effect.Param0 & 0x20) == 0x20,
                                IsDirectHit = (effect.Param0 & 0x40) == 0x40,
                                ActionId = actionId,
                            });
                            break;

                        case ActionEffectType.Heal:
                            OnActionResult?.Invoke(new ActionResultEvent
                            {
                                SourceId = resolvedSourceId,
                                SourceName = resolvedSourceName,
                                SourceJob = resolvedSourceJob,
                                TargetId = targetId,
                                TargetName = targetName,
                                TargetJob = targetJob,
                                IsPlayerTarget = isPlayerTarget,
                                Healing = amount,
                                IsCrit = (effect.Param1 & 0x20) == 0x20,
                                ActionId = actionId,
                            });
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Service.Logger.Error($"[Internal Parser] ActionEffect error: {ex.Message}");
        }
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

        if (target is IPlayerCharacter pc)
        {
            var job = Service.DataManager.GetExcelSheet<ClassJob>()
                .GetRowOrDefault(pc.ClassJob.RowId) ?? default;
            OnActionResult?.Invoke(new ActionResultEvent
            {
                SourceId = 0,
                SourceName = "DoT",
                SourceJob = default,
                TargetId = entityId,
                TargetName = pc.Name.TextValue,
                TargetJob = job,
                IsPlayerTarget = true,
                IsDamageTakenOnly = true,
                Damage = amount,
            });
            return;
        }

        var sources = statusTracker.GetDoTSources(entityId);
        if (sources.Count == 0) return;

        var splitAmount = (uint)(amount / sources.Count);
        foreach (var sourceId in sources)
        {
            if (Service.ObjectTable.SearchById(sourceId) is not IPlayerCharacter sourcePc) continue;

            var sourceJob = Service.DataManager.GetExcelSheet<ClassJob>()
                .GetRowOrDefault(sourcePc.ClassJob.RowId) ?? default;

            OnActionResult?.Invoke(new ActionResultEvent
            {
                SourceId = sourceId,
                SourceName = sourcePc.Name.TextValue,
                SourceJob = sourceJob,
                TargetId = entityId,
                TargetName = target?.Name.TextValue ?? "",
                TargetJob = default,
                IsPlayerTarget = false,
                Damage = splitAmount,
            });
        }
    }

    private void HandleHoTTick(uint entityId, uint statusId, uint amount)
    {
        if (Service.ObjectTable.SearchById(entityId) is not IPlayerCharacter pc) return;

        var targetJob = Service.DataManager.GetExcelSheet<ClassJob>()
            .GetRowOrDefault(pc.ClassJob.RowId) ?? default;

        ulong resolvedSourceId = entityId;
        string resolvedSourceName = pc.Name.TextValue;
        ClassJob resolvedSourceJob = targetJob;

        if (statusId != 0)
        {
            var sourceId = statusTracker.GetSource(entityId, statusId);
            if (sourceId != null &&
                Service.ObjectTable.SearchById(sourceId.Value) is IPlayerCharacter sourcePc)
            {
                resolvedSourceId = sourcePc.GameObjectId;
                resolvedSourceName = sourcePc.Name.TextValue;
                resolvedSourceJob = Service.DataManager.GetExcelSheet<ClassJob>()
                    .GetRowOrDefault(sourcePc.ClassJob.RowId) ?? default;
            }
        }

        OnActionResult?.Invoke(new ActionResultEvent
        {
            SourceId = resolvedSourceId,
            SourceName = resolvedSourceName,
            SourceJob = resolvedSourceJob,
            TargetId = entityId,
            TargetName = pc.Name.TextValue,
            TargetJob = targetJob,
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
