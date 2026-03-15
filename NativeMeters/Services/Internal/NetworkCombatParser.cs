using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Network;
using Lumina.Excel;
using NativeMeters.Configuration;
using NativeMeters.Models.Internal;
using BattleNpcSubKind = Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind;
using LuminaAction = Lumina.Excel.Sheets.Action;

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
    private static readonly ExcelSheet<LuminaAction> ActionSheet = Service.DataManager.GetExcelSheet<LuminaAction>();

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

    private unsafe bool IsEntityInFilter(ulong entityId)
    {
        var filter = System.Config.InternalParser.ParseFilter;
        if (filter == ParseFilter.None) return true;

        var localPlayer = Service.ObjectTable.LocalPlayer;
        if (localPlayer == null) return false;

        if (entityId == localPlayer.GameObjectId) return true;

        var entityObj = Service.ObjectTable.SearchById(entityId);
        uint ownerId = 0;
        if (entityObj != null && entityObj.OwnerId != InvalidGameObjectId && entityObj.OwnerId != 0)
        {
            ownerId = (uint)entityObj.OwnerId;
            if (ownerId == localPlayer.GameObjectId) return true;
        }

        if (filter == ParseFilter.Self) return false;

        var groupManager = GroupManager.Instance();
        if (groupManager == null) return false;

        uint objectId = (uint)entityId;

        bool inParty = groupManager->MainGroup.IsEntityIdInParty(objectId);
        bool inAlliance = groupManager->MainGroup.IsEntityIdInAlliance(objectId);

        if (filter == ParseFilter.Party && inParty) return true;
        if (filter == ParseFilter.Alliance && (inParty || inAlliance)) return true;

        if (ownerId != 0)
        {
            bool ownerInParty = groupManager->MainGroup.IsEntityIdInParty(ownerId);
            bool ownerInAlliance = groupManager->MainGroup.IsEntityIdInAlliance(ownerId);

            if (filter == ParseFilter.Party && ownerInParty) return true;
            if (filter == ParseFilter.Alliance && (ownerInParty || ownerInAlliance)) return true;
        }

        return false;
    }

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

            bool sourceInFilter = IsEntityInFilter(casterEntityId);

            if (casterPtr->GameObject.OwnerId != InvalidGameObjectId)
            {
                var owner = Service.ObjectTable.SearchById(casterPtr->GameObject.OwnerId);
                var casterObj = Service.ObjectTable.SearchById(casterEntityId);

                if (System.Config.InternalParser.ShowCompanions &&
                    casterObj is IBattleNpc npc && IsCompanionNpc(npc))
                {
                    resolvedSourceJobId = npc.ClassJob.RowId;
                }
                else if (System.Config.InternalParser.MergePetDamage && owner is IPlayerCharacter pcOwner)
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
                var action = ActionSheet.GetRowOrDefault(actionId);
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

                bool targetInFilter = IsEntityInFilter(targetId);

                if (!sourceInFilter && !targetInFilter) continue;

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

                    resolvedSourceName = GetResolvedName(resolvedSourceId, resolvedSourceName);

                    var targetName = GetResolvedName(targetId, targetObj.Name.TextValue);

                    var evt = new ActionResultEvent
                    {
                        SourceId = resolvedSourceId,
                        SourceName = resolvedSourceName,
                        SourceJobId = resolvedSourceJobId,
                        TargetId = targetId,
                        TargetName = targetName,
                        TargetCurrentHp = currentHp,
                        TargetMaxHp = maxHp,
                        TargetJobId = targetObj switch
                        {
                            IPlayerCharacter tpc => tpc.ClassJob.RowId,
                            IBattleNpc tnpc when System.Config.InternalParser.ShowCompanions
                                                 && IsCompanionNpc(tnpc) => tnpc.ClassJob.RowId,
                            _ => 0
                        },
                        IsPlayerTarget = targetObj is IPlayerCharacter ||
                                         (System.Config.InternalParser.ShowCompanions && IsCompanionNpc(targetObj)),
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
                    var deadObj = Service.ObjectTable.SearchById(entityId);
                    if (deadObj is IPlayerCharacter deadPc)
                    {
                        OnActorDeath?.Invoke(entityId, deadPc.Name.TextValue);
                    }
                    else if (System.Config.InternalParser.ShowCompanions && deadObj != null && IsCompanionNpc(deadObj))
                    {
                        OnActorDeath?.Invoke(entityId, deadObj.Name.TextValue);
                    }
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

        bool targetInFilter = IsEntityInFilter(entityId);

        if (statusId != 0)
        {
            var sourceId = statusTracker.GetSource(entityId, statusId);
            if (sourceId != null)
            {
                if (!IsEntityInFilter(sourceId.Value) && !targetInFilter) return;

                var sourceObj = Service.ObjectTable.SearchById((uint)sourceId);
                if (sourceObj is IPlayerCharacter pc)
                {
                    InvokeDoT(pc.GameObjectId, pc.Name.TextValue, pc.ClassJob.RowId,
                               entityId, target?.Name.TextValue ?? "", amount);
                    return;
                }
                if (System.Config.InternalParser.ShowCompanions &&
                    sourceObj is IBattleNpc npc && IsCompanionNpc(npc))
                {
                    InvokeDoT(npc.GameObjectId, npc.Name.TextValue, npc.ClassJob.RowId,
                               entityId, target?.Name.TextValue ?? "", amount);
                    return;
                }
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
            var sourceId = sources[0];
            if (!IsEntityInFilter(sourceId) && !targetInFilter) return;

            var sourceObj = Service.ObjectTable.SearchById((uint)sourceId);
            if (sourceObj is IPlayerCharacter pc)
            {
                InvokeDoT(sources[0], pc.Name.TextValue, pc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", amount);
            }
            else if (System.Config.InternalParser.ShowCompanions &&
                     sourceObj is IBattleNpc npc && IsCompanionNpc(npc))
            {
                InvokeDoT(sources[0], npc.Name.TextValue, npc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", amount);
            }
            return;
        }

        var splitAmount = (uint)(amount / sources.Count);
        foreach (var sourceId in sources)
        {
            if (!IsEntityInFilter(sourceId) && !targetInFilter) continue;

            var sourceObj = Service.ObjectTable.SearchById((uint)sourceId);
            if (sourceObj is IPlayerCharacter pc)
            {
                InvokeDoT(sourceId, pc.Name.TextValue, pc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", splitAmount);
            }
            else if (System.Config.InternalParser.ShowCompanions &&
                     sourceObj is IBattleNpc npc && IsCompanionNpc(npc))
            {
                InvokeDoT(sourceId, npc.Name.TextValue, npc.ClassJob.RowId,
                           entityId, target?.Name.TextValue ?? "", splitAmount);
            }
        }
    }

    private void InvokeDoT(ulong sourceId, string sourceName, uint sourceJobId, uint targetId, string targetName, uint amount)
    {
        OnActionResult?.Invoke(new ActionResultEvent
        {
            SourceId = sourceId,
            SourceName = GetResolvedName(sourceId, sourceName),
            SourceJobId = sourceJobId,
            TargetId = targetId,
            TargetName = GetResolvedName(targetId, targetName),
            IsPlayerTarget = false,
            Damage = amount,
            ActionId = 0,
        });
    }

    private void HandleHoTTick(uint entityId, uint statusId, uint amount)
    {
        var hotTarget = Service.ObjectTable.SearchById(entityId);
        if (hotTarget == null) return;

        string hotName;
        uint hotJobId;

        if (hotTarget is IPlayerCharacter pc)
        {
            hotName = pc.Name.TextValue;
            hotJobId = pc.ClassJob.RowId;
        }
        else if (System.Config.InternalParser.ShowCompanions &&
                 hotTarget is IBattleNpc npc && IsCompanionNpc(npc))
        {
            hotName = npc.Name.TextValue;
            hotJobId = npc.ClassJob.RowId;
        }
        else
        {
            return;
        }

        ulong resolvedSourceId = entityId;
        string resolvedSourceName = hotName;
        uint resolvedSourceJobId = hotJobId;

        if (statusId != 0)
        {
            var sourceId = statusTracker.GetSource(entityId, statusId);
            if (sourceId != null)
            {
                var sourceObj = Service.ObjectTable.SearchById(sourceId.Value);
                resolvedSourceId = sourceId.Value;

                if (sourceObj is IPlayerCharacter sourcePc)
                {
                    resolvedSourceName = sourcePc.Name.TextValue;
                    resolvedSourceJobId = sourcePc.ClassJob.RowId;
                }
                else if (System.Config.InternalParser.ShowCompanions &&
                         sourceObj is IBattleNpc sourceNpc && IsCompanionNpc(sourceNpc))
                {
                    resolvedSourceName = sourceNpc.Name.TextValue;
                    resolvedSourceJobId = sourceNpc.ClassJob.RowId;
                }
            }
        }

        bool targetInFilter = IsEntityInFilter(entityId);
        bool sourceInFilter = IsEntityInFilter(resolvedSourceId);

        if (!sourceInFilter && !targetInFilter) return;

        OnActionResult?.Invoke(new ActionResultEvent
        {
            SourceId = resolvedSourceId,
            SourceName = GetResolvedName(resolvedSourceId, resolvedSourceName),
            SourceJobId = resolvedSourceJobId,
            TargetId = entityId,
            TargetName = GetResolvedName(entityId, hotName),
            TargetJobId = hotJobId,
            IsPlayerTarget = true,
            Healing = amount,
        });
    }

    private string GetResolvedName(ulong id, string originalName)
    {
        if (System.Config.InternalParser.UseYouForLocalPlayer &&
            Service.ObjectTable.LocalPlayer != null &&
            id == Service.ObjectTable.LocalPlayer?.GameObjectId)
        {
            return "YOU";
        }
        return originalName;
    }

    private static bool IsCompanionNpc(IGameObject obj)
    {
        return obj is IBattleNpc { BattleNpcKind: BattleNpcSubKind.NpcPartyMember or BattleNpcSubKind.Chocobo };
    }

    public void Dispose()
    {
        actionEffectHook?.Dispose();
        actorControlHook?.Dispose();
        statusTracker.Clear();
        enabled = false;
    }
}
