using System;
using System.Collections.Generic;
using NativeMeters.Models;

namespace NativeMeters.Data.Stats;

public static class StatSelector
{
    // TODO: Add all / do it better.
    public static List<string> GetAvailableStatSelectors() =>
    [
        "ENCDPS",
        "ENCHPS",
        "DPS",
        "Damage%",
        "DirectHit%",
        "CritHit%",
        "Deaths"
    ];

    public static Func<Combatant, double> GetStatSelector(string statName) => statName.ToUpperInvariant() switch
    {
        "ENCDPS" => c => c.ENCDPS,
        "ENCHPS" => c => c.ENCHPS,
        "DPS" => c => c.DPS,
        "DAMAGE%" => c => c.DamagePercent,
        "DIRECTHIT%" => c => c.DirectHitPct,
        "CRITHIT%" => c => c.CrithitPercent,
        "DEATHS" => c => c.Deaths,
        _ => c => c.ENCDPS,
    };
}
