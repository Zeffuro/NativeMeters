using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Data.Stats;
using NativeMeters.Models;

namespace NativeMeters.Services;

public static class FakeEncounterFactory
{
    public static Encounter CreateFakeEncounter(IEnumerable<Combatant> combatants)
    {
        var list = combatants.ToList();
        if (list.Count == 0)
            throw new ArgumentException("Combatants collection is empty.");

        return new Encounter
        {
            // Metadata
            N = "Test Encounter",
            T = "Boss",
            Title = "Fake Boss",
            CurrentZoneName = "Fake Zone",

            Duration = Aggregator.AvgTimeSpan(list, c => c.Duration),
            DURATION = Aggregator.Avg(list, c => c.DURATION),

            DamageM = Aggregator.Sum(list, c => c.DamageM),
            DamageStar = Aggregator.Sum(list, c => c.DamageStar),
            DAMAGEK = Aggregator.Sum(list, c => c.DAMAGEK),
            DAMAGEM = Aggregator.Sum(list, c => c.DAMAGEM),
            DAMAGEB = Aggregator.Sum(list, c => c.DAMAGEB2),
            DAMAGEStar = Aggregator.Sum(list, c => c.DAMAGEStar),

            Dps = Aggregator.Sum(list, c => c.Dps),
            DpsStar = Aggregator.Sum(list, c => c.DpsStar),
            DPS = Aggregator.Sum(list, c => c.DPS),
            DPSK = Aggregator.Sum(list, c => c.DPSK),
            DPSM = Aggregator.Sum(list, c => c.DPSM),
            DPSStar = Aggregator.Sum(list, c => c.DPSStar),
            Encdps = Aggregator.Sum(list, c => c.Encdps),
            EncdpsStar = Aggregator.Sum(list, c => c.EncdpsStar),
            ENCDPS = Aggregator.Sum(list, c => c.ENCDPS),
            ENCDPSK = Aggregator.Sum(list, c => c.ENCDPSK),
            ENCDPSM = Aggregator.Sum(list, c => c.ENCDPSM),
            ENCDPSStar = Aggregator.Sum(list, c => c.ENCDPSStar),

            CrithitPercent = Aggregator.Avg(list, c => c.CrithitPercent),
            Tohit = Aggregator.Avg(list, c => c.Tohit),
            TOHIT = Aggregator.Avg(list, c => c.TOHIT),
            CrithealPercent = Aggregator.Avg(list, c => c.CrithealPercent),

            Maxhit = Aggregator.Max(list, c => c.Maxhit),
            MAXHIT = Aggregator.Max(list, c => c.MAXHIT),
            MaxhitStar = Aggregator.MaxString(list, c => c.MaxhitStar),
            MAXHITStar = Aggregator.MaxString(list, c => c.MAXHITStar),

            Enchps = Aggregator.Sum(list, c => c.Enchps),
            EnchpsStar = Aggregator.Sum(list, c => c.EnchpsStar),
            ENCHPS = Aggregator.Sum(list, c => c.ENCHPS),
            ENCHPSK = Aggregator.Sum(list, c => c.ENCHPSK),
            ENCHPSM = Aggregator.Sum(list, c => c.ENCHPSM),
            ENCHPSStar = Aggregator.Sum(list, c => c.ENCHPSStar),

            Maxheal = Aggregator.Max(list, c => c.Maxheal),
            MAXHEAL = Aggregator.Max(list, c => c.MAXHEAL),
            Maxhealward = Aggregator.Max(list, c => c.Maxhealward),
            MAXHEALWARD = Aggregator.Max(list, c => c.MAXHEALWARD),
            MaxhealStar = Aggregator.Max(list, c => c.MaxhealStar),
            MAXHEALStar = Aggregator.Max(list, c => c.MAXHEALStar),
            MaxhealwardStar = Aggregator.Max(list, c => c.MaxhealwardStar),
            MAXHEALWARDStar = Aggregator.Max(list, c => c.MAXHEALWARDStar),

            Damagetaken = Aggregator.Sum(list, c => c.Damagetaken),
            DamagetakenStar = Aggregator.Sum(list, c => c.DamagetakenStar),
            Healstaken = Aggregator.Sum(list, c => c.Healstaken),
            HealstakenStar = Aggregator.Sum(list, c => c.HealstakenStar),

            Powerdrain = Aggregator.Sum(list, c => c.Powerdrain),
            PowerdrainStar = Aggregator.Sum(list, c => c.PowerdrainStar),
            Powerheal = Aggregator.Sum(list, c => c.Powerheal),
            PowerhealStar = Aggregator.Sum(list, c => c.PowerhealStar),

            Last10DPS = Aggregator.Sum(list, c => c.Last10DPS),
            Last30DPS = Aggregator.Sum(list, c => c.Last30DPS),
            Last60DPS = Aggregator.Sum(list, c => c.Last60DPS),

            Damage = Aggregator.SumLong(list, c => c.Damage),
            Hits = Aggregator.SumInt(list, c => c.Hits),
            Crithits = Aggregator.SumInt(list, c => c.Crithits),
            Misses = Aggregator.SumInt(list, c => c.Misses),
            Hitfailed = Aggregator.SumInt(list, c => c.Hitfailed),
            Swings = Aggregator.SumInt(list, c => c.Swings),
            Healed = Aggregator.SumInt(list, c => c.Healed),
            Heals = Aggregator.SumInt(list, c => c.Heals),
            Critheals = Aggregator.SumInt(list, c => c.Critheals),
            Cures = Aggregator.SumInt(list, c => c.Cures),
            Kills = Aggregator.SumInt(list, c => c.Kills),
            Deaths = Aggregator.SumInt(list, c => c.Deaths)
        };
    }
}
