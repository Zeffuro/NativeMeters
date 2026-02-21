using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using NativeMeters.Models;
using NativeMeters.Models.Internal;
using Action = Lumina.Excel.Sheets.Action;

namespace NativeMeters.Services.Internal;

public class CombatantTracker(ulong actorId, string name, uint jobId)
{
    public ulong ActorId => actorId;
    public string Name => name;
    public uint JobId { get; set; } = jobId;
    public bool IsPlayer => JobId != 0;

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

    public DateTime? FirstActionTime { get; set; }
    public DateTime? LastActionTime { get; set; }

    public Dictionary<uint, ActionStat> ActionBreakdown { get; } = new();

    public void AddAction(ActionResultEvent evt)
    {
        var now = DateTime.UtcNow;
        FirstActionTime ??= now;
        LastActionTime = now;

        switch (evt)
        {
            case { Damage: > 0 }:
                TotalDamage += evt.Damage;
                Hits++;
                Swings++;
                if (evt.IsCrit) CritHits++;
                if (evt.IsDirectHit) DirectHits++;
                if (evt.IsCrit && evt.IsDirectHit) CritDirectHits++;
                if (evt.Damage > MaxHitValue) MaxHitValue = evt.Damage;
                UpdateBreakdown(evt, true);
                break;

            case { IsMiss: true }:
                Misses++;
                Swings++;
                break;

            case { Healing: > 0 }:
                long needed = (evt.TargetMaxHp > evt.TargetCurrentHp)
                    ? (evt.TargetMaxHp - evt.TargetCurrentHp) : 0;
                long overHealAmount = Math.Max(0, evt.Healing - needed);

                TotalHealing += evt.Healing;
                OverHeal += overHealAmount;
                HealCount++;
                if (evt.IsCrit) CritHeals++;
                if (evt.Healing > MaxHealValue) MaxHealValue = evt.Healing;
                UpdateBreakdown(evt, false);
                break;
        }
    }

    private void UpdateBreakdown(ActionResultEvent evt, bool isDamage)
    {
        if (!ActionBreakdown.TryGetValue(evt.ActionId, out var stat))
        {
            stat = new ActionStat { ActionId = evt.ActionId };
            ActionBreakdown[evt.ActionId] = stat;
        }

        var now = DateTime.UtcNow;
        stat.FirstUsed ??= now;
        stat.LastUsed = now;

        stat.Hits++;
        if (isDamage)
        {
            stat.TotalDamage += evt.Damage;
            if (evt.IsCrit) stat.CritHits++;
            if (evt.IsDirectHit) stat.DirectHits++;
            if (evt.Damage > stat.MaxHit) stat.MaxHit = evt.Damage;
        }
        else
        {
            stat.TotalHealing += evt.Healing;
            if (evt.IsCrit) stat.CritHits++;
            if (evt.Healing > stat.MaxHit) stat.MaxHit = evt.Healing;
        }
    }

    public void AddDamageTaken(ActionResultEvent evt)
    {
        if (evt.Damage > 0) DamageTaken += evt.Damage;
        if (evt.Healing > 0) HealsTaken += evt.Healing;
    }

    public Combatant ToCombatant(TimeSpan duration, long totalPartyDamage)
    {
        var job = Service.DataManager.GetExcelSheet<ClassJob>().GetRowOrDefault(JobId) ?? default;
        double seconds = Math.Max(duration.TotalSeconds, 1.0);
        double dps = TotalDamage / seconds;
        double hps = TotalHealing / seconds;
        double dmgPct = totalPartyDamage > 0 ? TotalDamage * 100.0 / totalPartyDamage : 0;

        return new Combatant
        {
            N = "\n",
            T = "\t",
            Name = Name,
            Job = job,
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

            ActiveTime = (FirstActionTime.HasValue && LastActionTime.HasValue)
                ? LastActionTime.Value - FirstActionTime.Value
                : null,

            ActionBreakdownList = ActionBreakdown.Values
                .Select(a =>
                {
                    var action = Service.DataManager.GetExcelSheet<Action>()
                        .GetRowOrDefault(a.ActionId);
                    return new ActionStatView
                    {
                        ActionId = a.ActionId,
                        ActionName = action?.Name.ToString() ?? $"Action {a.ActionId}",
                        ActionIconId = action?.Icon ?? 0,
                        TotalDamage = a.TotalDamage,
                        TotalHealing = a.TotalHealing,
                        Hits = a.Hits,
                        CritHits = a.CritHits,
                        DirectHits = a.DirectHits,
                        MaxHit = a.MaxHit,
                        DamagePerSecond = a.TotalDamage / seconds,
                        HealingPerSecond = a.TotalHealing / seconds,
                        FirstUsed = a.FirstUsed,
                        LastUsed = a.LastUsed,
                        ActiveSpan = (a.FirstUsed.HasValue && a.LastUsed.HasValue)
                            ? a.LastUsed.Value - a.FirstUsed.Value
                            : TimeSpan.Zero,
                        DamagePercent = TotalDamage > 0 ? a.TotalDamage * 100.0 / TotalDamage : 0,
                        HealingPercent = TotalHealing > 0 ? a.TotalHealing * 100.0 / TotalHealing : 0,
                    };
                })
                .OrderByDescending(a => a.TotalDamage)
                .ToList(),
        };
    }
}
