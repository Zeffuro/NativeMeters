using System;
using System.Collections.Generic;
using NativeMeters.Models;

namespace NativeMeters.Data.Stats;

public static class StatSelector
{
    public const string DefaultStatSelector = "ENCDPS";

    private static readonly List<string> AvailableStatSelectors =
    [
        DefaultStatSelector,
        "ENCHPS",
        "DPS",
        "Damage%",
        "DirectHit%",
        "CritHit%",
        "Deaths"
    ];

    // TODO: Add all / do it better.
    public static List<string> GetAvailableStatSelectors() =>
    [
        ..AvailableStatSelectors
    ];

    public static string NormalizeStatSelector(string? statName)
    {
        if (string.IsNullOrWhiteSpace(statName))
            return DefaultStatSelector;

        foreach (var selector in AvailableStatSelectors)
        {
            if (string.Equals(selector, statName, StringComparison.OrdinalIgnoreCase))
                return selector;
        }

        return DefaultStatSelector;
    }

    public static Func<Combatant, double> GetStatSelector(string statName) => statName.ToUpperInvariant() switch
    {
        DefaultStatSelector => c => c.ENCDPS,
        "ENCHPS" => c => c.ENCHPS,
        "DPS" => c => c.DPS,
        "DAMAGE%" => c => c.DamagePercent,
        "DIRECTHIT%" => c => c.DirectHitPct,
        "CRITHIT%" => c => c.CrithitPercent,
        "DEATHS" => c => c.Deaths,
        _ => c => c.ENCDPS,
    };
}
