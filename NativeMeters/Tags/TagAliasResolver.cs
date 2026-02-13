using System;
using System.Collections.Generic;

namespace NativeMeters.Tags;

public static class TagAliasResolver
{
    private static readonly Dictionary<string, (string Combatant, string Encounter)> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "name", ("Name", "Title") },
        { "hps", ("Enchps", "Enchps") },
        { "damage", ("Damage", "Damage") },
        { "damagetotal", ("Damage", "Damage") },
        { "damagepct", ("DamagePercent", "DamagePercent") },
        { "zone", ("CurrentZoneName", "CurrentZoneName") },
        { "duration", ("Duration", "Duration") },
        { "healedpct", ("HealedPercent", "HealedPercent") },
        { "crithitpct", ("CrithitPercent", "CrithitPercent") },
        { "crithealpct", ("CrithealPercent", "CrithealPercent") }
    };

    public static string Resolve(string key, bool isEncounter)
    {
        if (Aliases.TryGetValue(key, out var alias))
            return isEncounter ? alias.Encounter : alias.Combatant;

        return key;
    }
}
