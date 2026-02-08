using System;
using Lumina.Excel.Sheets;
using NativeMeters.Models;

namespace NativeMeters.Services.Internal;

public class CombatantTracker(ulong actorId, string name, ClassJob job)
{
    public ulong ActorId => actorId;
    public string Name => name;
    public ClassJob Job => job;
    public bool IsPlayer => job.RowId != 0;

    public DateTime? FirstAction { get; private set; }
    public DateTime? LastAction { get; private set; }

    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int Hits { get; set; }
    public int CritHits { get; set; }
    public int DirectHits { get; set; }
    public int CritDirectHits { get; set; }
    public int Swings { get; set; }
    public int HealCount { get; set; }
    public int CritHeals { get; set; }
    public long DamageTaken { get; set; }
    public long HealsTaken { get; set; }
    public long OverHeal { get; set; }
    public long MaxHitValue { get; set; }
    public long MaxHealValue { get; set; }
    public int Deaths { get; set; }

    private void UpdateActivity()
    {
        var now = DateTime.Now;
        FirstAction ??= now;
        LastAction = now;
    }

    public void AddAction(ActionResultEvent evt)
    {
        UpdateActivity();
        if (evt.Damage > 0)
        {
            TotalDamage += evt.Damage;
            Hits++;
            Swings++;
            if (evt.IsCrit) CritHits++;
            if (evt.IsDirectHit) DirectHits++;
            if (evt.IsCrit && evt.IsDirectHit) CritDirectHits++;
            if (evt.Damage > MaxHitValue) MaxHitValue = evt.Damage;
        }

        if (evt.Healing > 0)
        {
            TotalHealing += evt.Healing;
            HealCount++;
            if (evt.IsCrit) CritHeals++;
            if (evt.Healing > MaxHealValue) MaxHealValue = evt.Healing;
            if (evt.OverHeal > 0) OverHeal += evt.OverHeal;
        }
    }

    public void AddDamageTaken(ActionResultEvent evt)
    {
        if (evt.Damage > 0) DamageTaken += evt.Damage;
        if (evt.Healing > 0) HealsTaken += evt.Healing;
    }

    public double GetActiveDuration()
    {
        if (FirstAction == null || LastAction == null) return 0;
        var diff = (LastAction.Value - FirstAction.Value).TotalSeconds;
        return Math.Max(diff, 1.0);
    }

    public Combatant ToCombatant(TimeSpan encounterDuration)
    {
        double activeSeconds = GetActiveDuration();
        double encounterSeconds = Math.Max(encounterDuration.TotalSeconds, 1.0);

        double dps = TotalDamage / encounterSeconds;
        double hps = TotalHealing / encounterSeconds;

        return new Combatant
        {
            N = Name,
            Name = Name,
            Job = Job,
            Duration = TimeSpan.FromSeconds(activeSeconds),
            DURATION = activeSeconds,

            Damage = TotalDamage,
            Dps = dps,
            DPS = dps,
            Encdps = dps,
            ENCDPS = dps,

            DamageStar = TotalDamage,
            DAMAGEK = TotalDamage / 1000.0,
            DAMAGEM = TotalDamage / 1000000.0,

            Enchps = hps,
            ENCHPS = hps,

            Hits = Hits,
            Crithits = CritHits,
            Swings = Swings,
            Misses = Swings - Hits,

            CrithitPercent = Hits > 0 ? (CritHits * 100.0 / Hits) : 0,
            DirectHitPct = Hits > 0 ? (DirectHits * 100.0 / Hits) : 0,
            CritDirectHitPct = Hits > 0 ? (CritDirectHits * 100.0 / Hits) : 0,

            Maxhit = MaxHitValue,
            MAXHIT = MaxHitValue,
            MaxhitStar = $"{MaxHitValue}",
            MAXHITStar = $"{MaxHitValue}",

            Healed = (int)TotalHealing,
            Heals = HealCount,
            Critheals = CritHeals,
            OverHeal = OverHeal,
            OverHealPct = (TotalHealing + OverHeal) > 0 ? (OverHeal * 100.0 / (TotalHealing + OverHeal)) : 0,

            Deaths = Deaths,
            Damagetaken = DamageTaken,
            Healstaken = HealsTaken,
            IsActive = (DateTime.Now - (LastAction ?? DateTime.MinValue)).TotalSeconds < 6
        };
    }
}
