using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace NativeMeters.Services.Internal;

internal sealed class PeriodicDamageWeights(CombatDefinitions definitions)
{
    private const int MaxSamples = 16;
    private const long SampleLifetimeMs = 60000;

    public readonly record struct SourceKey(ulong Id, uint Job, byte Level);
    public readonly record struct WeightSnapshot(SourceKey? Source, decimal Potency, decimal? InitialScale, byte? DamageLowByte);

    private sealed class Baseline
    {
        public readonly Queue<decimal> Samples = new();
        public long LastSeen;
        public decimal Scale;
    }

    private readonly Dictionary<SourceKey, Baseline> baselines = new();
    private readonly Dictionary<uint, string?> actionCategories = new();
    private readonly LevelPotency levelPotency = new();
    private readonly Dictionary<(ulong Target, ulong Source, uint Status), StatusAmount> statusAmounts = new();
    private readonly ExcelSheet<LuminaAction> actionSheet = Service.DataManager.GetExcelSheet<LuminaAction>(ClientLanguage.English);

    private sealed class StatusAmount(byte amount)
    {
        public readonly byte Amount = amount;
        public readonly long AppliedAt = Environment.TickCount64;
        public long ExpiresAt;
    }

    public void ObserveStatus(uint targetId, ulong sourceId, uint statusId, byte amount)
    {
        if (definitions.GetStatus(statusId)?.Potency.Any(modifier => modifier.AmountByte == 2) != true) return;

        statusAmounts[(targetId, sourceId, statusId)] = new StatusAmount(amount);
    }

    public decimal? ObserveDirectHit(uint sourceId, uint targetId, uint actionId, uint amount)
    {
        var action = definitions.GetAction(actionId);
        if (action?.CanCalibrate != true) return null;

        if (amount == 0 || Service.ObjectTable.SearchById(sourceId) is not IBattleChara source ||
            Service.ObjectTable.SearchById(targetId) is not IBattleChara target) return null;

        var minimumLevel = action.CalibrationMinLevel ?? definitions.CalibrationMinLevel;
        var fromTooltip = source.Level < minimumLevel ? levelPotency.Get(actionId, source.ClassJob.RowId, source.Level, false) : null;
        var potency = action?.GetCalibrationPotency(source.Level, definitions.CalibrationMinLevel, fromTooltip);
        if (potency is not > 0) return null;

        var modifiers = GetMultiplier(source, target, actionId, "", out var supported);
        if (!supported || modifiers <= 0) return null;

        var key = new SourceKey(sourceId, source.ClassJob.RowId, source.Level);
        if (!baselines.TryGetValue(key, out var baseline)) baselines[key] = baseline = new();

        baseline.Samples.Enqueue(amount / (potency.Value * modifiers));
        while (baseline.Samples.Count > MaxSamples) baseline.Samples.Dequeue();
        baseline.LastSeen = Environment.TickCount64;

        Span<decimal> samples = stackalloc decimal[baseline.Samples.Count];
        var index = 0;
        foreach (var sample in baseline.Samples) samples[index++] = sample;
        samples.Sort();
        var middle = samples.Length / 2;
        baseline.Scale = samples.Length % 2 == 0 ? (samples[middle - 1] + samples[middle]) / 2 : samples[middle];
        return baseline.Scale;
    }

    public WeightSnapshot Snapshot(ulong sourceId, uint targetId, uint actionId, uint statusId, byte? damageLowByte = null)
    {
        var statusDefinition = definitions.GetStatus(statusId);
        var potencyActionId = statusDefinition?.ActionId is > 0 ? statusDefinition.ActionId : actionId;
        if (statusDefinition?.PotencyStatusByAction.Count > 0)
        {
            if (!statusDefinition.PotencyStatusByAction.TryGetValue(actionId, out var potencyStatus)) return default;
            statusDefinition = definitions.GetStatus(potencyStatus);
        }

        if (statusDefinition?.Disabled == true) return default;

        var definition = statusDefinition?.TimeProc;
        if (definition == null) return default;

        var source = Service.ObjectTable.SearchById(sourceId) as IBattleChara;
        var target = Service.ObjectTable.SearchById(targetId) as IBattleChara;
        var level = source?.Level ?? 0;
        var potency = definition.GetPotency(level);
        if (level < definitions.CalibrationMinLevel && !definition.Levels.Any(range => level >= range.MinLevel && level <= range.MaxLevel))
            potency = levelPotency.Get(potencyActionId, source?.ClassJob.RowId ?? 0, level, true, statusDefinition!.IsGround) ?? 0;
        if (potency <= 0) return default;

        var weighted = potency * GetMultiplier(source, target, actionId, definition.DamageType, out var supported);
        if (source == null || !supported) return new(null, weighted, null, null);

        var key = new SourceKey(sourceId, source.ClassJob.RowId, source.Level);
        return new(key, weighted, GetDamageScale(key), statusDefinition!.IsGround ? null : damageLowByte);
    }

    public decimal? GetDamageWeight(WeightSnapshot snapshot)
    {
        if (snapshot.Source is not { } source) return null;

        var scale = GetDamageScale(source) ?? snapshot.InitialScale;
        if (scale is not > 0) return null;

        var estimate = snapshot.Potency * scale.Value;
        if (snapshot.DamageLowByte is not { } lowByte) return estimate;

        // Use the packet residue only when normal damage variance leaves one possible value.
        var lower = Math.Max(1, estimate / 1.05m - 1);
        var upper = estimate / 0.95m + 1;
        var candidate = lowByte + 256 * decimal.Ceiling((lower - lowByte) / 256);
        return candidate <= upper && candidate + 256 > upper ? candidate : estimate;
    }

    private decimal? GetDamageScale(SourceKey key)
    {
        if (!baselines.TryGetValue(key, out var baseline) || Environment.TickCount64 - baseline.LastSeen > SampleLifetimeMs)
            return null;

        return baseline.Scale;
    }

    private decimal GetMultiplier(IBattleChara? source, IBattleChara? target, uint actionId, string damageType, out bool supported)
    {
        if (!actionCategories.TryGetValue(actionId, out var category))
            actionCategories[actionId] = category = actionSheet.GetRowOrDefault(actionId)?.ActionCategory.Value.Name.ToString();

        var sourceMultiplier = GetActorMultiplier(source, "damagedonemultiplier", category, damageType, out var sourceSupported);
        var targetMultiplier = GetActorMultiplier(target, "damagereceivedmultiplier", category, damageType, out var targetSupported);
        supported = sourceSupported && targetSupported;
        return sourceMultiplier * targetMultiplier;
    }

    private decimal GetActorMultiplier(IBattleChara? actor, string kind, string? category, string damageType, out bool supported)
    {
        supported = true;
        var multiplier = 1m;
        if (actor == null) return multiplier;

        Span<uint> seen = stackalloc uint[actor.StatusList.Length];
        var count = 0;
        foreach (var status in actor.StatusList)
        {
            if (status.StatusId == 0 || seen[..count].Contains(status.StatusId)) continue;
            seen[count++] = status.StatusId;

            var definition = definitions.GetStatus(status.StatusId);
            if (definition == null || definition.Disabled) continue;

            foreach (var modifier in definition.Potency)
            {
                if (modifier.Type.Equals("DamageAddPotency", StringComparison.OrdinalIgnoreCase))
                {
                    supported = false;
                    continue;
                }

                if (!modifier.Type.Equals(kind, StringComparison.OrdinalIgnoreCase)) continue;

                if (modifier.Extra.Count > 0 || (modifier.LimitToDamageType != null && damageType.Length == 0))
                {
                    supported = false;
                    continue;
                }

                if (modifier.LimitToDamageType != null && !modifier.LimitToDamageType.Equals(damageType, StringComparison.OrdinalIgnoreCase)) continue;

                if (modifier.LimitToActionCategory != null && category == null)
                {
                    supported = false;
                    continue;
                }

                if (modifier.LimitToActionCategory != null && !modifier.LimitToActionCategory.Equals(category, StringComparison.OrdinalIgnoreCase)) continue;

                var amount = modifier.Amount;
                if (modifier.AmountByte != null)
                {
                    if (modifier.AmountByte != 2 || !TryGetStatusAmount(actor.GameObjectId, status.SourceId, status.StatusId, status.RemainingTime, out amount))
                    {
                        supported = false;
                        continue;
                    }
                }

                multiplier *= Math.Max(0, 1 + amount / 100);
            }
        }

        return multiplier;
    }

    public void Update()
    {
        foreach (var (key, baseline) in baselines.ToArray())
            if (Environment.TickCount64 - baseline.LastSeen > SampleLifetimeMs) baselines.Remove(key);

        foreach (var (key, observation) in statusAmounts.ToArray())
        {
            if (Environment.TickCount64 - observation.AppliedAt < 2000) continue;

            if (Service.ObjectTable.SearchById(key.Target) is not IBattleChara actor)
            {
                statusAmounts.Remove(key);
                continue;
            }

            var present = false;
            foreach (var status in actor.StatusList)
            {
                if (status.StatusId != key.Status || status.SourceId != key.Source) continue;

                present = true;
                TryGetStatusAmount(key.Target, key.Source, key.Status, status.RemainingTime, out _);
                break;
            }

            if (!present) statusAmounts.Remove(key);
        }
    }

    private bool TryGetStatusAmount(ulong targetId, ulong sourceId, uint statusId, float remainingTime, out decimal amount)
    {
        amount = 0;
        var key = (targetId, sourceId, statusId);
        if (!statusAmounts.TryGetValue(key, out var observation) || !float.IsFinite(remainingTime) || remainingTime <= 0) return false;

        var now = Environment.TickCount64;
        var expiresAt = now + (long)(remainingTime * 1000);
        if (observation.ExpiresAt != 0 && expiresAt > observation.ExpiresAt + 750 && now - observation.AppliedAt > 2000)
        {
            statusAmounts.Remove(key);
            return false;
        }

        observation.ExpiresAt = expiresAt;
        amount = observation.Amount;
        return true;
    }

    public void Clear()
    {
        baselines.Clear();
        statusAmounts.Clear();
    }
}
