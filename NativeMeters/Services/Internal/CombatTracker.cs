using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.Sheets;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Models.Internal;

namespace NativeMeters.Services.Internal;

public class CombatTracker
{
    private readonly Dictionary<ulong, CombatantTracker> trackers = new();
    private EncounterState encounterState = new();
    private readonly Dictionary<string, long> enemyDamageTaken = new();

    private bool wasInCombat;
    private const double CombatIdleTimeoutSeconds = 3.0;

    public bool IsInCombat { get; private set; }
    public bool HasData => trackers.Count > 0 && encounterState.StartTime != DateTime.MinValue;
    public bool DidEncounterJustEnd { get; private set; }

    public void UpdateCombatState()
    {
        DidEncounterJustEnd = false;
        bool isInCombat = Service.Condition[ConditionFlag.InCombat];

        if (!wasInCombat && isInCombat)
        {
            if (!encounterState.IsActive)
            {
                Reset();
                encounterState.Start();
            }
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
        if (evt.Damage > 0)
        {
            if (!encounterState.IsActive)
            {
                Reset();
                encounterState.EnsureStarted();
            }
            encounterState.UpdateLastAction();
        }

        if (!encounterState.IsActive) return;

        if (!evt.IsDamageTakenOnly)
        {
            if (evt.IsLimitBreak)
            {
                var lbTracker = GetOrCreateTracker(0, "Limit Break", 0);
                lbTracker.AddAction(evt);
            }
            else
            {
                var source = GetOrCreateTracker(evt.SourceId, evt.SourceName, evt.SourceJobId);
                source.AddAction(evt);
            }
        }

        if (evt.TargetId != 0 && evt.IsPlayerTarget)
        {
            var target = GetOrCreateTracker(evt.TargetId, evt.TargetName, evt.TargetJobId);
            target.AddDamageTaken(evt);
        }

        if (evt.Damage > 0 && !evt.IsPlayerTarget && !string.IsNullOrEmpty(evt.TargetName))
        {
            enemyDamageTaken.TryGetValue(evt.TargetName, out var existing);
            enemyDamageTaken[evt.TargetName] = existing + evt.Damage;
            var strongest = enemyDamageTaken.MaxBy(kv => kv.Value);
            if (strongest.Value > 0) encounterState.EncounterName = strongest.Key;
        }
    }

    public void HandleDeath(ulong actorId, string actorName)
    {
        if (trackers.TryGetValue(actorId, out var tracker))
        {
            tracker.Deaths++;
        }
    }

    public Dictionary<string, Combatant> GetCombatants()
    {
        var duration = encounterState.GetDuration();
        var playerTrackers = trackers.Values
            .Where(tracker => tracker.IsPlayer || tracker.Name == "Limit Break")
            .ToList();
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
        double seconds = Math.Max(duration.TotalSeconds, 1.0);

        long totalDamage = 0;
        double sumEncdps = 0, sumEncdpsStar = 0, sumENCDPS = 0, sumENCDPSStar = 0;
        double sumEncdpsk = 0, sumEncdpsm = 0;
        double sumEnchps = 0, sumEnchpsStar = 0, sumENCHPS = 0, sumENCHPSStar = 0;
        double sumEnchpsk = 0, sumEnchpsm = 0;
        int totalHits = 0, totalCrithits = 0, totalMisses = 0, totalSwings = 0;
        int totalHealed = 0, totalHeals = 0, totalCritheals = 0;
        int totalDeaths = 0, totalKills = 0;
        double totalDamagetaken = 0, totalDamagetakenStar = 0;
        double totalHealstaken = 0, totalHealstakenStar = 0;

        foreach (var c in list)
        {
            totalDamage += c.Damage;
            sumEncdps += c.Encdps;
            sumEncdpsStar += c.EncdpsStar;
            sumENCDPS += c.ENCDPS;
            sumENCDPSStar += c.ENCDPSStar;
            sumEncdpsk += c.ENCDPSK;
            sumEncdpsm += c.ENCDPSM;
            sumEnchps += c.Enchps;
            sumEnchpsStar += c.EnchpsStar;
            sumENCHPS += c.ENCHPS;
            sumENCHPSStar += c.ENCHPSStar;
            sumEnchpsk += c.ENCHPSK;
            sumEnchpsm += c.ENCHPSM;
            totalHits += c.Hits;
            totalCrithits += c.Crithits;
            totalMisses += c.Misses;
            totalSwings += c.Swings;
            totalHealed += c.Healed;
            totalHeals += c.Heals;
            totalCritheals += c.Critheals;
            totalDeaths += c.Deaths;
            totalKills += c.Kills;
            totalDamagetaken += c.Damagetaken;
            totalDamagetakenStar += c.DamagetakenStar;
            totalHealstaken += c.Healstaken;
            totalHealstakenStar += c.HealstakenStar;
        }

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
            Encdps = sumEncdps,
            ENCDPS = sumENCDPS,
            EncdpsStar = sumEncdpsStar,
            ENCDPSStar = sumENCDPSStar,
            ENCDPSK = sumEncdpsk,
            ENCDPSM = sumEncdpsm,

            Enchps = sumEnchps,
            ENCHPS = sumENCHPS,
            EnchpsStar = sumEnchpsStar,
            ENCHPSStar = sumENCHPSStar,
            ENCHPSK = sumEnchpsk,
            ENCHPSM = sumEnchpsm,

            Hits = totalHits,
            Crithits = totalCrithits,
            Misses = totalMisses,
            Swings = totalSwings,
            Healed = totalHealed,
            Heals = totalHeals,
            Critheals = totalCritheals,

            Deaths = totalDeaths,
            Kills = totalKills,

            CrithitPercent = totalSwings > 0
                ? totalCrithits * 100.0 / totalSwings
                : 0,
            CrithealPercent = totalHeals > 0
                ? totalCritheals * 100.0 / totalHeals
                : 0,

            Damagetaken = totalDamagetaken,
            DamagetakenStar = totalDamagetakenStar,
            Healstaken = totalHealstaken,
            HealstakenStar = totalHealstakenStar,
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

    private CombatantTracker GetOrCreateTracker(ulong actorId, string name, uint jobId)
    {
        if (!trackers.TryGetValue(actorId, out var tracker))
        {
            tracker = new CombatantTracker(actorId, name, jobId);
            trackers[actorId] = tracker;
        }
        else
        {
            if (jobId != 0 && tracker.JobId != jobId)
                tracker.JobId = jobId;

            if (tracker.Name != name)
                tracker.Name = name;
        }

        return tracker;
    }
}
