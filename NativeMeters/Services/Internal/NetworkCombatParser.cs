using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
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

public unsafe partial class NetworkCombatParser : IDisposable
{
    public event Action<ActionResultEvent>? OnActionResult;
    public event Action<ulong, string>? OnActorDeath;
    public ulong UnattributedDotDamage { get; private set; }

    internal ParserCapture Capture { get; } = new();

    private const uint InvalidGameObjectId = 0xE0000000;
    private const uint MountActionOffset = 0xD000000;
    private const uint ItemActionOffset = 0x2000000;
    private const int MaxTrackedActions = 4096;

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
        AppliesToSource = 0x80,
    }

    private readonly StatusTracker statusTracker = new();
    private readonly Queue<PendingTick> pendingTicks = new();
    private readonly record struct PendingTick(uint TargetId, uint StatusId, uint Amount, long ObservedAt, DateTime TimestampUtc, uint SourceId);
    private readonly HashSet<(uint Source, uint Sequence, uint Target, int Index)> seenActions = new();
    private readonly Queue<(uint Source, uint Sequence, uint Target, int Index)> actionOrder = new();

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

    public void ResetTracking()
    {
        if (Capture.IsActive) Capture.Record("TrackingReset", new { UnattributedDotDamage });

        statusTracker.Clear();
        pendingTicks.Clear();
        seenActions.Clear();
        actionOrder.Clear();
        UnattributedDotDamage = 0;
    }

    public void UpdateTracking() => statusTracker.Update();

    public void ProcessPendingTicks(bool flush = false, uint? targetId = null)
    {
        for (var i = pendingTicks.Count; i > 0; i--)
        {
            var tick = pendingTicks.Dequeue();
            var age = Environment.TickCount64 - tick.ObservedAt;
            if ((targetId != null && tick.TargetId != targetId) || (!flush && age < 150))
            {
                pendingTicks.Enqueue(tick);
                continue;
            }

            try
            {
                if (!flush && age < 2000 && statusTracker.HasPendingApplication(tick.TargetId, tick.StatusId, tick.ObservedAt))
                {
                    pendingTicks.Enqueue(tick);
                    continue;
                }

                HandleDoTTick(tick);
            }
            catch (Exception ex)
            {
                Service.Logger.Error(ex, "Could not process a deferred periodic tick.");
            }
        }
    }

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

    private void ActorControlDetour(
        uint entityId, uint category, uint arg1, uint arg2,
        uint arg3, uint arg4, uint arg5, uint arg6,
        uint arg7, uint arg8, GameObjectId targetId, bool isRecorded)
    {
        var receivedAt = DateTime.UtcNow;
        var observedAt = Environment.TickCount64;
        actorControlHook!.Original(entityId, category, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, targetId, isRecorded);

        try
        {
            switch ((ActorControlCategory)category)
            {
                case ActorControlCategory.DoT:
                    if (Capture.IsActive)
                    {
                        Capture.Record("ActorControlTick", new
                        {
                            EntityId = entityId, Category = category, TargetId = targetId.Id,
                            Arg1 = arg1, Arg2 = arg2, Arg3 = arg3, Arg4 = arg4,
                            Arg5 = arg5, Arg6 = arg6, Arg7 = arg7, Arg8 = arg8,
                            TimestampUtc = receivedAt,
                        });
                    }

                    var tick = new PendingTick(entityId, arg1, arg2, observedAt, receivedAt, arg3);
                    if (!statusTracker.HasGroundSource(arg1, arg3) &&
                        statusTracker.HasPendingApplication(entityId, arg1, observedAt) && pendingTicks.Count < 1024)
                        pendingTicks.Enqueue(tick);
                    else
                        HandleDoTTick(tick);
                    break;

                case ActorControlCategory.HoT:
                    HandleHoTTick(entityId, arg1, arg2);
                    break;

                case ActorControlCategory.Death:
                    ProcessPendingTicks(true, entityId);
                    statusTracker.RemoveTarget(entityId);
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

    private void HandleDoTTick(PendingTick tick)
    {
        var (entityId, statusId, amount, observedAt, timestampUtc, packetSourceId) = tick;
        var target = Service.ObjectTable.SearchById(entityId);
        var allocations = statusTracker.AllocateTick(entityId, statusId, amount, observedAt, packetSourceId);
        if (Capture.IsActive)
            Capture.Record("PeriodicTick", new { TargetId = entityId, StatusId = statusId, Amount = amount, Allocations = allocations, TimestampUtc = timestampUtc });

        if (allocations.Count == 0)
        {
            UnattributedDotDamage += amount;
            return;
        }

        var targetInFilter = IsEntityInFilter(entityId);
        foreach (var allocation in allocations)
        {
            if (allocation.Amount == 0) continue;

            if (!TryResolveDotSource(allocation.SourceId, out var sourceId, out var sourceName, out var sourceJobId))
            {
                UnattributedDotDamage += allocation.Amount;
                continue;
            }

            if (!IsEntityInFilter(allocation.SourceId) && !targetInFilter) continue;

            EmitActionResult(new ActionResultEvent
            {
                TimestampUtc = timestampUtc,
                SourceId = sourceId,
                SourceName = GetResolvedName(sourceId, sourceName),
                SourceJobId = sourceJobId,
                TargetId = entityId,
                TargetName = GetResolvedName(entityId, target?.Name.TextValue ?? ""),
                TargetJobId = target switch
                {
                    IPlayerCharacter player => player.ClassJob.RowId,
                    IBattleNpc npc when System.Config.InternalParser.ShowCompanions && IsCompanionNpc(npc)
                        => npc.ClassJob.RowId,
                    _ => 0,
                },
                IsPlayerTarget = target is IPlayerCharacter ||
                                 (target != null && System.Config.InternalParser.ShowCompanions && IsCompanionNpc(target)),
                Damage = allocation.Amount,
                ActionId = allocation.ActionId,
                IsPeriodic = true,
                IsEstimated = allocation.Estimated,
            });
        }
    }

    private static bool TryResolveDotSource(ulong actorId, out ulong sourceId, out string name, out uint jobId)
    {
        sourceId = actorId;
        name = "";
        jobId = 0;
        if (actorId is 0 or InvalidGameObjectId) return false;

        var source = Service.ObjectTable.SearchById(actorId);
        if (source == null) return false;

        if (source is IPlayerCharacter player)
        {
            name = player.Name.TextValue;
            jobId = player.ClassJob.RowId;
            return true;
        }

        if (System.Config.InternalParser.ShowCompanions && source is IBattleNpc npc && IsCompanionNpc(npc))
        {
            name = npc.Name.TextValue;
            jobId = npc.ClassJob.RowId;
            return true;
        }

        if (System.Config.InternalParser.MergePetDamage && source.OwnerId is not (0 or InvalidGameObjectId) &&
            Service.ObjectTable.SearchById(source.OwnerId) is IPlayerCharacter owner)
        {
            sourceId = owner.GameObjectId;
            name = owner.Name.TextValue;
            jobId = owner.ClassJob.RowId;
            return true;
        }

        name = source.Name.TextValue;
        return true;
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

        EmitActionResult(new ActionResultEvent
        {
            SourceId = resolvedSourceId,
            SourceName = GetResolvedName(resolvedSourceId, resolvedSourceName),
            SourceJobId = resolvedSourceJobId,
            TargetId = entityId,
            TargetName = GetResolvedName(entityId, hotName),
            TargetJobId = hotJobId,
            IsPlayerTarget = true,
            Healing = amount,
            IsPeriodic = true,
        });
    }

    private void EmitActionResult(ActionResultEvent result)
    {
        if (result.TimestampUtc == default) result = result with { TimestampUtc = DateTime.UtcNow };
        if (Capture.IsActive) Capture.Record("Action", result);

        OnActionResult?.Invoke(result);
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
        return obj is IBattleNpc { BattleNpcKind: BattleNpcSubKind.NpcPartyMember or BattleNpcSubKind.Buddy };
    }

    public void Dispose()
    {
        actionEffectHook?.Dispose();
        actorControlHook?.Dispose();
        actionEffectHook = null;
        actorControlHook = null;
        statusTracker.Clear();
        Capture.Dispose();
        enabled = false;
    }
}
