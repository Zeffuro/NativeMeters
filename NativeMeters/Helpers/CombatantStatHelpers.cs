using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Models;

namespace NativeMeters.Helpers;

public static class CombatantStatHelpers
{
    // TODO: Add all / do it better.
    private static readonly Dictionary<string, Func<Combatant, string>> StatRegistry = new()
    {
        { "Name", c => c.Name },
        { "Job", c => c.Job.Abbreviation.ExtractText() },
        { "ENCDPS", c => c.ENCDPS.ToString("N0") },
        { "ENCHPS", c => c.ENCHPS.ToString("N0") },
        { "DPS", c => c.DPS.ToString("N0") },
        { "Damage%", c => $"{c.DamagePercent:0.0}%" },
        { "DirectHit%", c => $"{c.DirectHitPct:0.0}%" },
        { "CritHit%", c => $"{c.CrithitPercent:0.0}%" },
        { "CritDirectHit%", c => $"{c.CritDirectHitPct:0.0}%" },
        { "Deaths", c => c.Deaths.ToString() },
        { "MaxHit", c => c.Maxhit.ToString("N0") },
    };

    public static List<string> GetAvailableStatSources() => StatRegistry.Keys.ToList();

    public static string GetStatValueByName(Combatant combatant, string format)
    {
        return TagProcessor.Process(format, combatant);
    }

    public static string GetGlobalStatValue(string format)
    {
        var encounter = System.ActiveMeterService.GetEncounter();
        return TagProcessor.Process(format, encounter);
    }

    public static Func<Combatant, double> GetStatSelector(string statName) => statName switch
    {
        "ENCDPS" => c => c.ENCDPS,
        "ENCHPS" => c => c.ENCHPS,
        "DPS" => c => c.DPS,
        "Damage%" => c => c.DamagePercent,
        _ => c => c.ENCDPS,
    };
}