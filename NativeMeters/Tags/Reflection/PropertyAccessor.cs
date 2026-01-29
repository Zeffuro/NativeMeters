using System;
using System.Collections.Generic;
using System.Reflection;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Tags.Reflection;

public static class PropertyAccessor
{
    private static readonly Dictionary<string, PropertyInfo> CombatantCaseSensitive = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, PropertyInfo> CombatantCaseInsensitive = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PropertyInfo> EncounterCaseSensitive = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, PropertyInfo> EncounterCaseInsensitive = new(StringComparer.OrdinalIgnoreCase);

    static PropertyAccessor()
    {
        foreach (var prop in typeof(Combatant).GetProperties())
        {
            CombatantCaseSensitive[prop.Name] = prop;
            CombatantCaseInsensitive.TryAdd(prop.Name, prop);
        }

        foreach (var prop in typeof(Encounter).GetProperties())
        {
            EncounterCaseSensitive[prop.Name] = prop;
            EncounterCaseInsensitive.TryAdd(prop.Name, prop);
        }
    }

    public static object? GetValue(string key, object data)
    {
        var isEncounter = data is Encounter;
        var mappedKey = TagAliasResolver.Resolve(key, isEncounter);

        var caseSensitive = isEncounter ? EncounterCaseSensitive : CombatantCaseSensitive;
        var caseInsensitive = isEncounter ? EncounterCaseInsensitive : CombatantCaseInsensitive;

        PropertyInfo? prop;
        if (!caseSensitive.TryGetValue(mappedKey, out prop))
            caseInsensitive.TryGetValue(mappedKey, out prop);

        var value = prop?.GetValue(data);

        if (System.Config.General.PrivacyMode &&
            key.Equals("name", StringComparison.OrdinalIgnoreCase) &&
            data is Combatant combatant)
        {
            return combatant.Job.NameEnglish;
        }

        if (!isEncounter && key.Equals("name", StringComparison.OrdinalIgnoreCase) &&
            value is string s && s.Equals("YOU", StringComparison.OrdinalIgnoreCase))
        {
            if (System.Config.General.ReplaceYou)
                return Service.ObjectTable.LocalPlayer?.Name.TextValue ?? "YOU";
        }

        return value;
    }
}
