using System;
using System.Collections.Generic;
using NativeMeters.Extensions;
using NativeMeters.Models;

namespace NativeMeters.Services;

public static class FakeCombatantFactory
{
    private static readonly Random _random = new();

    public static Combatant CreateFakeCombatant(string name, int index)
    {
        // Helper for random double with 2 decimals
        double RandDouble(double min, double max) => Math.Round(_random.NextDouble() * (max - min) + min, 2);
        TimeSpan RandDuration(double minSeconds, double maxSeconds) => TimeSpan.FromSeconds(_random.NextDouble() * (maxSeconds - minSeconds) + minSeconds);

        // Helper for random string damage
        string RandDamageString() => RandDouble(10000, 5000000).ToString("N0");

        return new Combatant
        {
            N = index.ToString(),
            T = "Player",
            Name = name,
            Duration = RandDuration(60, 600),
            DURATION = RandDuration(60, 600),
            DamageM = RandDouble(0, 10),
            DamageB = RandDouble(0, 1),
            DamageStar = RandDouble(0, 10),
            DAMAGEK = RandDouble(0, 1000),
            DAMAGEM = RandDouble(0, 10),
            DAMAGEB2 = RandDouble(0, 1),
            DAMAGEStar = RandDouble(0, 10),
            DamagePercent = RandDouble(0, 100),
            Dps = RandDouble(1000, 50000),
            DpsStar = RandDouble(1000, 50000),
            DPS = RandDouble(1000, 50000),
            DPSK = RandDouble(1, 100),
            DPSM = RandDouble(0, 10),
            DPSStar = RandDouble(1000, 50000),
            Encdps = RandDouble(1000, 50000),
            EncdpsStar = RandDouble(1000, 50000),
            ENCDPS = RandDouble(1000, 50000),
            ENCDPSK = RandDouble(1, 100),
            ENCDPSM = RandDouble(0, 10),
            ENCDPSStar = RandDouble(1000, 50000),
            CrithitPercent = RandDouble(0, 100),
            Tohit = RandDouble(80, 100),
            TOHIT = RandDouble(80, 100),
            Maxhit = RandDouble(10000, 500000),
            MAXHIT = RandDouble(10000, 500000),
            MaxhitStar = $"{RandDouble(10000, 500000)} Crit",
            MAXHITStar = $"{RandDouble(10000, 500000)} Crit",
            HealedPercent = RandDouble(0, 100),
            Enchps = RandDouble(0, 10000),
            EnchpsStar = RandDouble(0, 10000),
            ENCHPS = RandDouble(0, 10000),
            ENCHPSK = RandDouble(0, 10),
            ENCHPSM = RandDouble(0, 1),
            ENCHPSStar = RandDouble(0, 10000),
            CrithealPercent = RandDouble(0, 100),
            Maxheal = RandDouble(1000, 100000),
            MAXHEAL = RandDouble(1000, 100000),
            Maxhealward = RandDouble(1000, 100000),
            MAXHEALWARD = RandDouble(1000, 100000),
            MaxhealStar = RandDouble(1000, 100000),
            MAXHEALStar = RandDouble(1000, 100000),
            MaxhealwardStar = RandDouble(1000, 100000),
            MAXHEALWARDStar = RandDouble(1000, 100000),
            Damagetaken = RandDouble(0, 100000),
            DamagetakenStar = RandDouble(0, 100000),
            Healstaken = RandDouble(0, 100000),
            HealstakenStar = RandDouble(0, 100000),
            Powerdrain = RandDouble(0, 10000),
            PowerdrainStar = RandDouble(0, 10000),
            Powerheal = RandDouble(0, 10000),
            PowerhealStar = RandDouble(0, 10000),
            Last10DPS = RandDouble(1000, 50000),
            Last30DPS = RandDouble(1000, 50000),
            Last60DPS = RandDouble(1000, 50000),
            ParryPct = RandDouble(0, 100),
            BlockPct = RandDouble(0, 100),
            IncToHit = RandDouble(80, 100),
            OverHealPct = RandDouble(0, 100),
            DirectHitPct = RandDouble(0, 100),
            CritDirectHitPct = RandDouble(0, 100),
            OverHeal = RandDouble(0, 10000),
            DamageShield = RandDouble(0, 10000),
            AbsorbHeal = RandDouble(0, 10000),
            Damage = _random.Next(10000, 5000000),
            Hits = _random.Next(10, 1000),
            Crithits = _random.Next(0, 100),
            Misses = _random.Next(0, 10),
            Hitfailed = _random.Next(0, 5),
            Swings = _random.Next(10, 1000),
            Healed = _random.Next(0, 10000),
            Critheals = _random.Next(0, 100),
            Heals = _random.Next(0, 1000),
            Cures = _random.Next(0, 10),
            Kills = _random.Next(0, 10),
            Deaths = _random.Next(0, 5),
            DirectHitCount = _random.Next(0, 100),
            CritDirectHitCount = _random.Next(0, 100),
            IsActive = _random.Next(0, 2) == 1,
            CritTypes = "Direct, Crit",
            ThreatStr = RandDamageString(),
            ThreatDelta = RandDamageString(),
            Job = Service.DataManager.GetClassJobByAbbreviation("PLD")
        };
    }

    public static List<Combatant> CreateFixedCombatants(int count)
    {
        var jobs = new[] { "PLD", "WAR", "WHM", "BLM", "DRG", "BRD", "NIN", "SCH" };
        var names = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Heidi" };
        var combatants = new List<Combatant>();
        for (int i = 0; i < count; i++)
        {
            string uniqueName = i < names.Length ? names[i] : $"{names[i % names.Length]} {i}";

            var combatant = CreateFakeCombatant(uniqueName, i + 1);
            var jobAbbr = jobs[i % jobs.Length];
            combatant.Job = Service.DataManager.GetClassJobByAbbreviation(jobAbbr);
            combatants.Add(combatant);
        }
        return combatants;
    }

    public static Dictionary<string, Combatant> CreateFakeCombatants(int count)
    {
        var combatants = new Dictionary<string, Combatant>();
        for (int i = 0; i < count; i++)
        {
            var name = $"Player{_random.Next(1, 100)}";
            combatants[name] = CreateFakeCombatant(name, i + 1);
        }
        return combatants;
    }
}