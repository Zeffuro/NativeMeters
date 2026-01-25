using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Models;

namespace NativeMeters.Services;

public static class FakeEncounterFactory
{
    private static string MaxHitString(IEnumerable<string> values)
    {
        double ParseNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            // Find the last "word" that looks like a number (with optional K/M/B)
            var parts = s.Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                var part = parts[i].Trim().ToUpperInvariant();
                double multiplier = 1;
                if (part.EndsWith("K")) { multiplier = 1_000; part = part[..^1]; }
                else if (part.EndsWith("M")) { multiplier = 1_000_000; part = part[..^1]; }
                else if (part.EndsWith("B")) { multiplier = 1_000_000_000; part = part[..^1]; }
                if (double.TryParse(part, global::System.Globalization.NumberStyles.Any, global::System.Globalization.CultureInfo.InvariantCulture, out var num))
                    return num * multiplier;
            }
            return 0;
        }

        return values
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new { Value = s, Num = ParseNumber(s) })
            .OrderByDescending(x => x.Num)
            .FirstOrDefault()?.Value ?? "";
    }

    public static Encounter CreateFakeEncounter(IEnumerable<Combatant> combatants)
    {
        var list = combatants.ToList();
        if (list.Count == 0)
            throw new ArgumentException("Combatants collection is empty.");

        double Avg(Func<Combatant, double> selector) => list.Average(selector);
        TimeSpan AvgTs(Func<Combatant, TimeSpan> selector) => TimeSpan.FromSeconds(list.Average(c => selector(c).TotalSeconds));
        int SumInt(Func<Combatant, int> selector) => list.Sum(selector);
        long SumLong(Func<Combatant, long> selector) => list.Sum(selector);
        double Sum(Func<Combatant, double> selector) => list.Sum(selector);

        return new Encounter
        {
            N = "Test Encounter",
            T = "Boss",
            Title = "Fake Boss",
            CurrentZoneName = "Fake Zone",

            Duration = AvgTs(c => c.Duration),
            DURATION = Avg(c => c.DURATION),

            DamageM = Sum(c => c.DamageM),
            DamageStar = Sum(c => c.DamageStar),
            DAMAGEK = Sum(c => c.DAMAGEK),
            DAMAGEM = Sum(c => c.DAMAGEM),
            DAMAGEB = Sum(c => c.DAMAGEB2),
            DAMAGEStar = Sum(c => c.DAMAGEStar),

            Dps = Sum(c => c.Dps),
            DpsStar = Sum(c => c.DpsStar),
            DPS = Sum(c => c.DPS),
            DPSK = Sum(c => c.DPSK),
            DPSM = Sum(c => c.DPSM),
            DPSStar = Sum(c => c.DPSStar),

            Encdps = Sum(c => c.Encdps),
            EncdpsStar = Sum(c => c.EncdpsStar),
            ENCDPS = Sum(c => c.ENCDPS),
            ENCDPSK = Sum(c => c.ENCDPSK),
            ENCDPSM = Sum(c => c.ENCDPSM),
            ENCDPSStar = Sum(c => c.ENCDPSStar),

            CrithitPercent = Avg(c => c.CrithitPercent),
            Tohit = Avg(c => c.Tohit),
            TOHIT = Avg(c => c.TOHIT),

            Maxhit = list.Max(c => c.Maxhit),
            MAXHIT = list.Max(c => c.MAXHIT),
            MaxhitStar = MaxHitString(list.Select(c => c.MaxhitStar)),
            MAXHITStar = MaxHitString(list.Select(c => c.MAXHITStar)),

            Enchps = Sum(c => c.Enchps),
            EnchpsStar = Sum(c => c.EnchpsStar),
            ENCHPS = Sum(c => c.ENCHPS),
            ENCHPSK = Sum(c => c.ENCHPSK),
            ENCHPSM = Sum(c => c.ENCHPSM),
            ENCHPSStar = Sum(c => c.ENCHPSStar),

            CrithealPercent = Avg(c => c.CrithealPercent),

            Maxheal = list.Max(c => c.Maxheal),
            MAXHEAL = list.Max(c => c.MAXHEAL),
            Maxhealward = list.Max(c => c.Maxhealward),
            MAXHEALWARD = list.Max(c => c.MAXHEALWARD),
            MaxhealStar = list.Max(c => c.MaxhealStar),
            MAXHEALStar = list.Max(c => c.MAXHEALStar),
            MaxhealwardStar = list.Max(c => c.MaxhealwardStar),
            MAXHEALWARDStar = list.Max(c => c.MAXHEALWARDStar),

            Damagetaken = Sum(c => c.Damagetaken),
            DamagetakenStar = Sum(c => c.DamagetakenStar),
            Healstaken = Sum(c => c.Healstaken),
            HealstakenStar = Sum(c => c.HealstakenStar),

            Powerdrain = Sum(c => c.Powerdrain),
            PowerdrainStar = Sum(c => c.PowerdrainStar),
            Powerheal = Sum(c => c.Powerheal),
            PowerhealStar = Sum(c => c.PowerhealStar),

            Last10DPS = Sum(c => c.Last10DPS),
            Last30DPS = Sum(c => c.Last30DPS),
            Last60DPS = Sum(c => c.Last60DPS),

            Damage = SumLong(c => c.Damage),
            Hits = SumInt(c => c.Hits),
            Crithits = SumInt(c => c.Crithits),
            Misses = SumInt(c => c.Misses),
            Hitfailed = SumInt(c => c.Hitfailed),
            Swings = SumInt(c => c.Swings),
            Healed = SumInt(c => c.Healed),
            Heals = SumInt(c => c.Heals),
            Critheals = SumInt(c => c.Critheals),
            Cures = SumInt(c => c.Cures),
            Kills = SumInt(c => c.Kills),
            Deaths = SumInt(c => c.Deaths)
        };
    }
}
