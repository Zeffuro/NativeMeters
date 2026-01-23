using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Lumina.Excel.Sheets;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Helpers;

public static class TagProcessor
{
    private static readonly Regex TagRegex = new(@"\[(?<key>[^:\]\._]+)(?:_(?<subKey>[^:\]\.]+))?(?::(?<format>[^\]\.]+))?(?:\.(?<precision>\d+))?\]", RegexOptions.Compiled);

    private static readonly Dictionary<string, PropertyInfo> CombatantCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PropertyInfo> EncounterCache = new(StringComparer.OrdinalIgnoreCase);

    static TagProcessor()
    {
        foreach (var prop in typeof(Combatant).GetProperties()) CombatantCache[prop.Name] = prop;
        foreach (var prop in typeof(Encounter).GetProperties()) EncounterCache[prop.Name] = prop;
    }

    public static string Process(string input, object data)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return TagRegex.Replace(input, match =>
        {
            var key = match.Groups["key"].Value;
            var subKey = match.Groups["subKey"].Value;
            var format = match.Groups["format"].Value;
            var precisionStr = match.Groups["precision"].Value;
            int? precision = string.IsNullOrEmpty(precisionStr) ? null : int.Parse(precisionStr);

            var rawValue = GetRawValue(key, data);
            if (rawValue == null) return match.Value;

            return FormatValue(rawValue, subKey, format, precision);
        });
    }

    private static object? GetRawValue(string key, object data)
    {
        var isEncounter = data is Encounter;

        var mappedKey = key.ToLower() switch
        {
            "name" => isEncounter ? "Title" : "Name",
            "hps" => "Enchps",
            "damage" or "damagetotal" => "Damage",
            "zone" => "CurrentZoneName",
            _ => key
        };

        var cache = isEncounter ? EncounterCache : CombatantCache;
        var value = cache.TryGetValue(mappedKey, out var prop) ? prop.GetValue(data) : null;

        if (!isEncounter && key.Equals("name", StringComparison.OrdinalIgnoreCase) && value is string s && s.Equals("YOU", StringComparison.OrdinalIgnoreCase))
        {
            if (System.Config.General.ReplaceYou)
                return Service.ObjectTable.LocalPlayer?.Name.TextValue ?? "YOU";
        }

        return value;
    }

    private static string FormatValue(object value, string subKey, string format, int? precision)
    {
        if (subKey.Equals("skill", StringComparison.OrdinalIgnoreCase) || subKey.Equals("val", StringComparison.OrdinalIgnoreCase))
        {
            var str = value.ToString() ?? "";
            var parts = str.Split('-');
            if (parts.Length >= 3)
            {
                if (subKey.Equals("skill", StringComparison.OrdinalIgnoreCase)) return parts[1];
                if (subKey.Equals("val", StringComparison.OrdinalIgnoreCase)) value = parts[2];
            }
            else if (parts.Length == 2)
            {
                if (subKey.Equals("val", StringComparison.OrdinalIgnoreCase)) value = parts[1];
            }
        }

        switch (value)
        {
            case string s:
                if (!string.IsNullOrEmpty(subKey))
                {
                    var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (subKey.Equals("first", StringComparison.OrdinalIgnoreCase))
                        s = parts.Length > 0 ? parts[0] : s;
                    else if (subKey.Equals("last", StringComparison.OrdinalIgnoreCase))
                        s = parts.Length > 1 ? parts[1] : (parts.Length > 0 ? string.Empty : s);
                }
                return (precision.HasValue && s.Length > precision.Value) ? s[..precision.Value] : s;

            case ClassJob job:
                return precision == 3 ? job.Abbreviation.ToString() : job.NameEnglish.ToString();
        }

        if (value is double or long or int || (value is string ns && double.TryParse(ns, NumberStyles.Any, CultureInfo.InvariantCulture, out _)))
        {
            double num = Convert.ToDouble(value);

            if (format.Equals("k", StringComparison.OrdinalIgnoreCase))
            {
                num /= 1000.0;
                return num.ToString($"F{precision ?? 0}", CultureInfo.InvariantCulture) + "k";
            }
            if (format.Equals("m", StringComparison.OrdinalIgnoreCase))
            {
                num /= 1000000.0;
                return num.ToString($"F{precision ?? 0}", CultureInfo.InvariantCulture) + "m";
            }

            string style = format.Equals("r", StringComparison.OrdinalIgnoreCase) ? "F" : "N";
            return num.ToString(style + (precision ?? 0), CultureInfo.InvariantCulture);
        }

        if (value is TimeSpan ts)
            return ts.ToString(precision == 1 ? @"mm\:ss" : @"m\:ss");

        return value?.ToString() ?? string.Empty;
    }
}
