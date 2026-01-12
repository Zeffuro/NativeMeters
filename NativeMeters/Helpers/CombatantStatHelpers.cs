using System;
using NativeMeters.Models;

namespace NativeMeters.Helpers;

public static class CombatantStatHelpers
{
    public static Func<Combatant, double> GetStatSelector(string statName) => statName switch
    {
        "ENCDPS" => c => c.ENCDPS,
        "ENCHPS" => c => c.ENCHPS,
        "DPS" => c => c.DPS,
        "Damage%" => c => c.DamagePercent,
        _ => c => c.ENCDPS,
    };

    public static string FormatStatValue(double value, string statName)
    {
        return statName.Contains('%') ? $"{value:0.0}%" : value.ToString("N0");
    }
}