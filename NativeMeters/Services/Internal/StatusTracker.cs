using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

public class StatusTracker
{
    private const long ConfirmationTimeoutMs = 2000;
    private const long TickGraceMs = 250;
    private readonly Dictionary<uint, Dictionary<ApplicationKey, Application>> targets = new();
    private readonly Dictionary<uint, bool> damageStatuses = new();
    private readonly ExcelSheet<Status> statusSheet = Service.DataManager.GetExcelSheet<Status>(ClientLanguage.English);
    private readonly CombatDefinitions definitions = new();
    private readonly PeriodicDamageWeights weights;
    private readonly record struct ApplicationKey(ulong SourceId, uint StatusId);

    private sealed class Application
    {
        public uint ActionId;
        public uint Sequence;
        public long AppliedAt;
        public long ExpiresAt;
        public long MissingAt;
        public bool Confirmed;
        public PeriodicDamageWeights.WeightSnapshot Snapshot;
        public Application? Previous;
        public decimal AverageTick;
        public int Samples;
    }

    public readonly record struct Allocation(ulong SourceId, uint ActionId, uint StatusId,
        uint Amount, bool Estimated, decimal Weight, string Basis);

    public StatusTracker() => weights = new(definitions);

    public decimal? ObserveDirectHit(uint sourceId, uint targetId, uint actionId, uint amount)
        => weights.ObserveDirectHit(sourceId, targetId, actionId, amount);

    private Application CreateApplication(uint targetId, ulong sourceId, uint statusId, uint actionId, uint sequence, byte? damageLowByte = null)
    {
        return new Application
        {
            ActionId = actionId,
            Sequence = sequence,
            AppliedAt = Environment.TickCount64,
            Snapshot = weights.Snapshot(sourceId, targetId, actionId, statusId, damageLowByte),
        };
    }

    public void ObserveApplication(uint targetId, ulong sourceId, uint statusId, uint actionId, uint sequence, byte amount)
    {
        weights.ObserveStatus(targetId, sourceId, statusId, amount);
        if (!IsDamageStatus(statusId)) return;

        if (!targets.TryGetValue(targetId, out var applications)) targets[targetId] = applications = new();
        var key = new ApplicationKey(sourceId, statusId);
        if (applications.TryGetValue(key, out var existing) && sequence != 0 && existing.Sequence == sequence) return;

        var application = CreateApplication(targetId, sourceId, statusId, actionId, sequence, amount);
        application.Previous = existing?.Confirmed == true ? existing : existing?.Previous;
        if (application.Previous != null) application.Previous.Previous = null;
        applications[key] = application;
    }

    public bool HasPendingApplication(uint targetId, uint statusId, long observedAt)
    {
        Synchronize(targetId, observedAt);
        if (definitions.GetStatus(statusId) is { IsGround: true, ApplicationSide: "source" })
        {
            foreach (var sourceTarget in targets.Where(pair => pair.Value.Keys.Any(key => key.StatusId == statusId)).Select(pair => pair.Key).ToArray())
                Synchronize(sourceTarget, observedAt);
        }

        var applications = definitions.GetStatus(statusId) is { IsGround: true, ApplicationSide: "source" }
            ? targets.Values.SelectMany(entries => entries)
            : targets.TryGetValue(targetId, out var entries) ? entries : Enumerable.Empty<KeyValuePair<ApplicationKey, Application>>();

        var pending = false;
        foreach (var (key, application) in applications)
        {
            if (statusId == 0 ? definitions.GetStatus(key.StatusId)?.IsGround == true : key.StatusId != statusId) continue;
            if (GetActiveApplication(application, observedAt) != null) return false;

            pending |= !application.Confirmed && application.AppliedAt <= observedAt &&
                       observedAt - application.AppliedAt < ConfirmationTimeoutMs;
        }

        return pending;
    }

    private static bool IsActiveAt(Application application, long observedAt)
        => application.Confirmed && application.AppliedAt <= observedAt && observedAt <= application.ExpiresAt + TickGraceMs &&
           (application.MissingAt == 0 || observedAt <= application.MissingAt);

    private static Application? GetActiveApplication(Application application, long observedAt)
    {
        if (application.Confirmed) return IsActiveAt(application, observedAt) ? application : null;
        return application.Previous is { } previous && IsActiveAt(previous, observedAt) ? previous : null;
    }

    public bool HasGroundSource(uint statusId, ulong sourceId)
        => statusId != 0 && definitions.GetStatus(statusId)?.IsGround == true &&
           sourceId is not (0 or 0xE0000000) && Service.ObjectTable.SearchById(sourceId) is IBattleChara;

    public List<Allocation> AllocateTick(uint targetId, uint statusId, uint amount, long observedAt, ulong sourceId = 0)
    {
        if (amount == 0) return new();

        if (HasGroundSource(statusId, sourceId))
        {
            var definition = definitions.GetStatus(statusId)!;
            return [new(sourceId, definition.ActionId, statusId, amount, false, 1, "Source")];
        }

        if (statusId != 0 && statusSheet.GetRowOrDefault(statusId) is { StatusCategory: 2 } && definitions.GetStatus(statusId) == null)
            damageStatuses[statusId] = true;

        Synchronize(targetId, observedAt);
        var candidates = new List<KeyValuePair<ApplicationKey, Application>>();
        if (targets.TryGetValue(targetId, out var applications))
        {
            foreach (var (key, application) in applications)
            {
                if (statusId == 0 ? definitions.GetStatus(key.StatusId)?.IsGround == true : key.StatusId != statusId) continue;
                if (GetActiveApplication(application, observedAt) is { } active) candidates.Add(new(key, active));
            }
        }
        candidates.Sort((left, right) => left.Key.SourceId != right.Key.SourceId
            ? left.Key.SourceId.CompareTo(right.Key.SourceId) : left.Key.StatusId.CompareTo(right.Key.StatusId));

        if (candidates.Count == 0 && definitions.GetStatus(statusId) is { IsGround: true, ApplicationSide: "source" })
            return AllocateGroundTick(statusId, amount, observedAt);
        if (candidates.Count == 0) return new();

        if (candidates.Count == 1)
        {
            var sample = candidates[0].Value;
            sample.Samples++;
            sample.AverageTick += (amount - sample.AverageTick) / sample.Samples;
        }

        var (values, basis) = GetWeights(candidates.Select(pair => pair.Value).ToArray());
        var amounts = SplitAmount(amount, values);
        return candidates.Select((pair, i) => new Allocation(pair.Key.SourceId, pair.Value.ActionId,
            pair.Key.StatusId, amounts[i], candidates.Count > 1, values[i], basis)).ToList();
    }

    private (decimal[] Values, string Basis) GetWeights(Application[] applications)
    {
        var values = new decimal[applications.Length];
        var calibrated = 0;
        var knownDamage = 0m;
        var knownPotency = 0m;
        for (var i = 0; i < applications.Length; i++)
        {
            var snapshot = applications[i].Snapshot;
            var estimate = weights.GetDamageWeight(snapshot);
            if (estimate is not > 0) continue;

            var damage = estimate.Value;
            values[i] = damage;
            knownDamage += damage;
            knownPotency += snapshot.Potency;
            calibrated++;
        }

        if (calibrated == applications.Length) return (values, "Damage");

        if (applications.All(application => application.Snapshot.Potency > 0))
        {
            var scale = knownPotency > 0 ? knownDamage / knownPotency : 1;
            for (var i = 0; i < applications.Length; i++)
                if (values[i] == 0) values[i] = applications[i].Snapshot.Potency * scale;

            return (values, calibrated > 0 ? "Mixed" : "Potency");
        }

        if (applications.All(application => application.Samples >= 2 && application.AverageTick > 0))
            return (applications.Select(application => application.AverageTick).ToArray(), "Observed");

        return (Enumerable.Repeat(1m, applications.Length).ToArray(), "Equal");
    }

    private static uint[] SplitAmount(uint amount, decimal[] weights)
    {
        var totalWeight = weights.Sum();
        var amounts = new uint[weights.Length];
        var remainders = new decimal[weights.Length];
        ulong allocated = 0;

        for (var i = 0; i < weights.Length; i++)
        {
            var share = amount * weights[i] / totalWeight;
            amounts[i] = (uint)decimal.Floor(share);
            remainders[i] = share - amounts[i];
            allocated += amounts[i];
        }

        foreach (var index in Enumerable.Range(0, weights.Length).OrderByDescending(i => remainders[i]))
        {
            if (allocated >= amount) break;

            amounts[index]++;
            allocated++;
        }

        return amounts;
    }

    private List<Allocation> AllocateGroundTick(uint statusId, uint amount, long observedAt)
    {
        Update();
        foreach (var actor in Service.ObjectTable)
            if (actor is IBattleChara battle && battle.StatusList.Any(status => status.StatusId == statusId))
                Synchronize((uint)actor.GameObjectId, observedAt);

        var candidates = targets.Values.SelectMany(applications => applications)
            .Where(pair => pair.Key.StatusId == statusId && IsActiveAt(pair.Value, observedAt))
            .GroupBy(pair => pair.Key.SourceId).ToArray();
        if (candidates.Length != 1) return new();

        var application = candidates[0].OrderByDescending(pair => pair.Value.AppliedAt).First();
        return [new Allocation(application.Key.SourceId, application.Value.ActionId, statusId, amount, true, 1, "UniqueSource")];
    }

    public void Update()
    {
        weights.Update();
        foreach (var targetId in targets.Keys.ToArray()) Synchronize(targetId);
    }

    private void Synchronize(uint targetId, long? observedAt = null)
    {
        if (Service.ObjectTable.SearchById(targetId) is not IBattleChara target)
        {
            targets.Remove(targetId);
            return;
        }

        if (!targets.TryGetValue(targetId, out var applications)) targets[targetId] = applications = new();
        var now = Environment.TickCount64;
        var present = new HashSet<ApplicationKey>();

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == 0 || !float.IsFinite(status.RemainingTime) ||
                status.RemainingTime <= 0 || !IsDamageStatus(status.StatusId)) continue;

            var key = new ApplicationKey(status.SourceId, status.StatusId);
            present.Add(key);
            var expiresAt = now + (long)(status.RemainingTime * 1000);
            if (!applications.TryGetValue(key, out var application) ||
                (application.Confirmed && expiresAt > application.ExpiresAt + 750 && now - application.AppliedAt > ConfirmationTimeoutMs))
            {
                var actionId = definitions.GetStatus(status.StatusId)?.ActionId ?? 0;
                application = CreateApplication(targetId, status.SourceId, status.StatusId, actionId, 0);
                application.AppliedAt = observedAt ?? now;
                application.Snapshot = application.Snapshot with { Source = null, InitialScale = null, DamageLowByte = null };
                applications[key] = application;
            }

            if (!application.Confirmed && application.Previous is { } previous &&
                expiresAt <= previous.ExpiresAt + 750 && now - application.AppliedAt < ConfirmationTimeoutMs)
                continue;

            application.Confirmed = true;
            application.Previous = null;
            application.ExpiresAt = expiresAt;
            application.MissingAt = 0;
        }

        foreach (var (key, application) in applications.ToArray())
        {
            if (present.Contains(key)) continue;

            if (application.MissingAt == 0) application.MissingAt = now;
            if (application.Previous is { MissingAt: 0 } previous) previous.MissingAt = now;

            var removeAt = application.Confirmed
                ? Math.Min(application.ExpiresAt, application.MissingAt) + TickGraceMs * 2
                : application.AppliedAt + ConfirmationTimeoutMs;
            if (now > removeAt) applications.Remove(key);
        }

        if (applications.Count == 0) targets.Remove(targetId);
    }

    private bool IsDamageStatus(uint statusId)
    {
        var definition = definitions.GetStatus(statusId);
        if (definition != null) return definition.IsDamage;
        if (damageStatuses.TryGetValue(statusId, out var result)) return result;

        var status = statusSheet.GetRowOrDefault(statusId);
        result = status is { StatusCategory: 2 } &&
                 status.Value.Description.ToString().Contains("damage over time", StringComparison.OrdinalIgnoreCase);
        damageStatuses[statusId] = result;
        return result;
    }

    public ulong? GetSource(uint targetId, uint statusId)
    {
        if (Service.ObjectTable.SearchById(targetId) is not IBattleChara target) return null;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId != statusId || status.SourceId is 0 or 0xE0000000) continue;
            return status.SourceId;
        }

        return null;
    }

    public void RemoveTarget(uint targetId) => targets.Remove(targetId);

    public void Clear()
    {
        targets.Clear();
        damageStatuses.Clear();
        weights.Clear();
    }
}
