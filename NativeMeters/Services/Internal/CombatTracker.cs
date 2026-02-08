using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.Sheets;
using NativeMeters.Models;

namespace NativeMeters.Services.Internal;

public class CombatTracker
{
    private readonly Dictionary<ulong, CombatantTracker> trackers = new();
    private EncounterState encounterState = new();
    private readonly Dictionary<string, long> enemyDamageTaken = new();

    private bool wasInCombat;

    public bool IsInCombat { get; private set; }
    public bool HasData => trackers.Count > 0 && encounterState.StartTime != DateTime.MinValue;
    public bool DidEncounterJustEnd { get; private set; }

    public void UpdateCombatState()
    {
        DidEncounterJustEnd = false;
        bool isInCombat = Service.Condition[ConditionFlag.InCombat];

        if (!wasInCombat && isInCombat)
        {
            Reset();
            encounterState.Start();
        }
        else if (wasInCombat && !isInCombat)
        {
            encounterState.End();
            DidEncounterJustEnd = true;
        }

        IsInCombat = isInCombat;
        wasInCombat = isInCombat;
    }

    public void HandleActionResult(ActionResultEvent evt)
    {
        if (!encounterState.IsActive) return;

        if (evt.IsDamageTakenOnly)
        {
            if (evt.IsPlayerTarget && evt.TargetId != 0)
            {
                var target = GetOrCreateTracker(evt.TargetId, evt.TargetName, evt.TargetJob);
                target.AddDamageTaken(evt);
            }
            return;
        }

        var source = GetOrCreateTracker(evt.SourceId, evt.SourceName, evt.SourceJob);
        source.AddAction(evt);

        if (evt.TargetId != 0 && evt.IsPlayerTarget)
        {
            var target = GetOrCreateTracker(evt.TargetId, evt.TargetName, evt.TargetJob);
            target.AddDamageTaken(evt);
        }

        if (evt.Damage > 0 && !evt.IsPlayerTarget && !string.IsNullOrEmpty(evt.TargetName))
        {
            enemyDamageTaken.TryGetValue(evt.TargetName, out var existing);
            enemyDamageTaken[evt.TargetName] = existing + evt.Damage;

            var strongest = enemyDamageTaken.MaxBy(kv => kv.Value);
            if (!string.IsNullOrEmpty(strongest.Key))
            {
                encounterState.EncounterName = strongest.Key;
            }
        }
    }

    public void HandleDeath(uint actorId, string actorName)
    {
        if (trackers.TryGetValue(actorId, out var tracker))
        {
            tracker.Deaths++;
        }
    }

    public Dictionary<string, Combatant> GetCombatants()
    {
        var duration = encounterState.GetDuration();
        var playerTrackers = trackers.Values.Where(tracker => tracker.IsPlayer).ToList();
        var totalPartyDamage = playerTrackers.Sum(tracker => tracker.TotalDamage);

        return playerTrackers.ToDictionary(
            tracker => tracker.Name,
            tracker => tracker.ToCombatant(duration, totalPartyDamage)
        );
    }

    public Encounter BuildEncounter(IEnumerable<Combatant> combatants)
    {
        var list = combatants.ToList();
        var duration = encounterState.GetDuration();
        var totalDamage = list.Sum(c => c.Damage);
        var totalHealing = list.Sum(c => (long)c.Healed);
        double seconds = Math.Max(duration.TotalSeconds, 1.0);

        return new Encounter
        {
            N = encounterState.EncounterName ?? "Unknown",
            T = "",
            Title = encounterState.EncounterName ?? "Unknown",
            CurrentZoneName = encounterState.ZoneName ?? "Unknown",

            Duration = duration,
            DURATION = duration.TotalSeconds,

            Damage = totalDamage,
            DamageM = totalDamage / 1_000_000.0,
            DamageStar = totalDamage,
            DAMAGEK = totalDamage / 1_000.0,
            DAMAGEM = totalDamage / 1_000_000.0,

            Dps = totalDamage / seconds,
            DPS = totalDamage / seconds,
            DPSK = totalDamage / seconds / 1_000.0,
            DPSM = totalDamage / seconds / 1_000_000.0,
            DpsStar = totalDamage / seconds,
            DPSStar = totalDamage / seconds,
            Encdps = list.Sum(c => c.Encdps),
            ENCDPS = list.Sum(c => c.ENCDPS),
            EncdpsStar = list.Sum(c => c.EncdpsStar),
            ENCDPSStar = list.Sum(c => c.ENCDPSStar),
            ENCDPSK = list.Sum(c => c.ENCDPSK),
            ENCDPSM = list.Sum(c => c.ENCDPSM),

            Enchps = list.Sum(c => c.Enchps),
            ENCHPS = list.Sum(c => c.ENCHPS),
            EnchpsStar = list.Sum(c => c.EnchpsStar),
            ENCHPSStar = list.Sum(c => c.ENCHPSStar),
            ENCHPSK = list.Sum(c => c.ENCHPSK),
            ENCHPSM = list.Sum(c => c.ENCHPSM),

            Hits = list.Sum(c => c.Hits),
            Crithits = list.Sum(c => c.Crithits),
            Misses = list.Sum(c => c.Misses),
            Swings = list.Sum(c => c.Swings),
            Healed = list.Sum(c => c.Healed),
            Heals = list.Sum(c => c.Heals),
            Critheals = list.Sum(c => c.Critheals),

            Deaths = list.Sum(c => c.Deaths),
            Kills = list.Sum(c => c.Kills),

            CrithitPercent = list.Sum(c => c.Swings) > 0
                ? list.Sum(c => c.Crithits) * 100.0 / list.Sum(c => c.Swings)
                : 0,
            CrithealPercent = list.Sum(c => c.Heals) > 0
                ? list.Sum(c => c.Critheals) * 100.0 / list.Sum(c => c.Heals)
                : 0,

            Damagetaken = list.Sum(c => c.Damagetaken),
            DamagetakenStar = list.Sum(c => c.DamagetakenStar),
            Healstaken = list.Sum(c => c.Healstaken),
            HealstakenStar = list.Sum(c => c.HealstakenStar),
        };
    }

    public void ForceEndEncounter()
    {
        encounterState.End();
        DidEncounterJustEnd = true;
    }

    public void Reset()
    {
        trackers.Clear();
        enemyDamageTaken.Clear();
        encounterState = new EncounterState();
    }

    private CombatantTracker GetOrCreateTracker(ulong actorId, string name, ClassJob job)
    {
        if (!trackers.TryGetValue(actorId, out var tracker))
        {
            tracker = new CombatantTracker(actorId, name, job);
            trackers[actorId] = tracker;
        }
        else if (job.RowId != 0 && tracker.Job.RowId != job.RowId)
        {
            tracker.Job = job;
        }

        return tracker;
    }
}
