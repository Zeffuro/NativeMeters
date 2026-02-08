using System;
using Lumina.Excel.Sheets;
using NativeMeters.Models;

namespace NativeMeters.Services.Internal;

public class CombatantTracker(ulong actorId, string name, ClassJob job)
{
    public ulong ActorId => actorId;
    public string Name => name;
    public ClassJob Job { get; set; } = job;
    public bool IsPlayer => Job.RowId != 0;

    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int Hits { get; set; }
    public int CritHits { get; set; }
    public int DirectHits { get; set; }
    public int CritDirectHits { get; set; }
    public int Misses { get; set; }
    public int Swings { get; set; }
    public int HealCount { get; set; }
    public int CritHeals { get; set; }
    public int Deaths { get; set; }
    public int Kills { get; set; }
    public long DamageTaken { get; set; }
    public long HealsTaken { get; set; }
    public long OverHeal { get; set; }
    public long MaxHitValue { get; set; }
    public long MaxHealValue { get; set; }

    public void AddAction(ActionResultEvent evt)
    {
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

    public Combatant ToCombatant(TimeSpan duration, long totalPartyDamage)
    {
        double seconds = Math.Max(duration.TotalSeconds, 1.0);
        double dps = TotalDamage / seconds;
        double hps = TotalHealing / seconds;
        double dmgPct = totalPartyDamage > 0 ? TotalDamage * 100.0 / totalPartyDamage : 0;

        return new Combatant
        {
            N = "\n",
            T = "\t",
            Name = Name,
            Job = Job,
            Duration = duration,
            DURATION = seconds,

            Damage = TotalDamage,
            DamageM = TotalDamage / 1_000_000.0,
            DamageStar = TotalDamage,
            DAMAGEK = TotalDamage / 1_000.0,
            DAMAGEM = TotalDamage / 1_000_000.0,
            DamagePercent = dmgPct,

            Dps = dps,
            DPS = dps,
            DPSK = dps / 1_000.0,
            DPSM = dps / 1_000_000.0,
            DpsStar = dps,
            DPSStar = dps,
            Encdps = dps,
            EncdpsStar = dps,
            ENCDPS = dps,
            ENCDPSK = dps / 1_000.0,
            ENCDPSM = dps / 1_000_000.0,
            ENCDPSStar = dps,

            Enchps = hps,
            EnchpsStar = hps,
            ENCHPS = hps,
            ENCHPSK = hps / 1_000.0,
            ENCHPSM = hps / 1_000_000.0,
            ENCHPSStar = hps,

            Hits = Hits,
            Crithits = CritHits,
            Misses = Misses,
            Swings = Swings,

            CrithitPercent = Swings > 0 ? (CritHits * 100.0 / Swings) : 0,
            Tohit = Swings > 0 ? (Hits * 100.0 / Swings) : 0,
            TOHIT = Swings > 0 ? (Hits * 100.0 / Swings) : 0,

            Maxhit = MaxHitValue,
            MAXHIT = MaxHitValue,
            MaxhitStar = MaxHitValue > 0 ? MaxHitValue.ToString() : "",
            MAXHITStar = MaxHitValue > 0 ? MaxHitValue.ToString() : "",

            Healed = (int)TotalHealing,
            Heals = HealCount,
            Critheals = CritHeals,
            CrithealPercent = HealCount > 0 ? (CritHeals * 100.0 / HealCount) : 0,
            Maxheal = MaxHealValue,
            MAXHEAL = MaxHealValue,

            Deaths = Deaths,
            Kills = Kills,

            Damagetaken = DamageTaken,
            DamagetakenStar = DamageTaken,
            Healstaken = HealsTaken,
            HealstakenStar = HealsTaken,

            DirectHitCount = DirectHits,
            CritDirectHitCount = CritDirectHits,
            DirectHitPct = Hits > 0 ? (DirectHits * 100.0 / Hits) : 0,
            CritDirectHitPct = Hits > 0 ? (CritDirectHits * 100.0 / Hits) : 0,
            OverHeal = OverHeal,

            IsActive = true,
        };
    }
}
