using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

    private static readonly Dictionary<PropertyInfo, Func<object, object?>> CompiledGetters = new();

    static PropertyAccessor()
    {
        foreach (var prop in typeof(Combatant).GetProperties())
        {
            CombatantCaseSensitive[prop.Name] = prop;
            CombatantCaseInsensitive.TryAdd(prop.Name, prop);
            CompiledGetters.TryAdd(prop, CompileGetter(prop));
        }

        foreach (var prop in typeof(Encounter).GetProperties())
        {
            EncounterCaseSensitive[prop.Name] = prop;
            EncounterCaseInsensitive.TryAdd(prop.Name, prop);
            CompiledGetters.TryAdd(prop, CompileGetter(prop));
        }
    }

    private static Func<object, object?> CompileGetter(PropertyInfo prop)
    {
        var parameter = Expression.Parameter(typeof(object), "obj");
        var cast = Expression.Convert(parameter, prop.DeclaringType!);
        var propertyAccess = Expression.Property(cast, prop);
        var boxed = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<object, object?>>(boxed, parameter).Compile();
    }

    public static object? GetValue(string key, object data)
    {
        var isEncounter = data is Encounter;
        var mappedKey = TagAliasResolver.Resolve(key, isEncounter);

        var caseSensitive = isEncounter ? EncounterCaseSensitive : CombatantCaseSensitive;
        var caseInsensitive = isEncounter ? EncounterCaseInsensitive : CombatantCaseInsensitive;

        if (!caseSensitive.TryGetValue(mappedKey, out var prop))
            caseInsensitive.TryGetValue(mappedKey, out prop);

        if (prop == null)
            return null;

        var value = CompiledGetters.TryGetValue(prop, out var getter) ? getter(data) : prop.GetValue(data);

        if (System.Config.General.PrivacyMode &&
            key.Equals("name", StringComparison.OrdinalIgnoreCase) &&
            data is Combatant combatant)
        {
            var jobName = combatant.Job.NameEnglish.ToString();
            return combatant.PrivacyIndex.HasValue
                ? $"{jobName} {combatant.PrivacyIndex.Value}"
                : jobName;
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
