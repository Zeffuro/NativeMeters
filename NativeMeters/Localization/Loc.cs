using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace NativeMeters.Localization;

internal static class Loc
{
    private static readonly ResourceManager Resources = new("NativeMeters.Localization.Strings", typeof(Loc).Assembly);
    private static string gameLanguage = "English";
    private static CultureInfo uiCulture = CultureInfo.GetCultureInfo("en");

    internal static IReadOnlyList<string> LanguageChoices { get; } =
        ["English", "Japanese", "German", "French", "Korean", "ChineseSimplified", "TraditionalChinese"];

    internal static string? LanguageOverride { get; private set; }
    internal static string DisplayLanguage => LanguageOverride ?? gameLanguage;

    internal static void Initialize(string clientLanguage)
    {
        gameLanguage = clientLanguage;
        SetLanguage(null);
    }

    internal static void SetLanguage(string? language)
    {
        LanguageOverride = language is not null && LanguageChoices.Contains(language) ? language : null;
        uiCulture = CultureInfo.GetCultureInfo(ResolveLanguage(DisplayLanguage));
    }

    internal static string ResolveLanguage(string clientLanguage)
        => clientLanguage switch
        {
            "Japanese" => "ja",
            "German" => "de",
            "French" => "fr",
            "Korean" => "ko",
            "ChineseSimplified" => "zh-CN",
            "ChineseTraditional" or "TraditionalChinese" => "zh-TW",
            _ => "en",
        };

    internal static string Get(string key) => GetOptional(key) ?? key;

    internal static (string Title, string? Subtitle) GetWindowHeader(string key)
    {
        var localized = Get(key);
        if (uiCulture.Name is not ("ja" or "ko" or "zh-CN" or "zh-TW"))
            return (localized, null);

        var english = Resources.GetString(key, CultureInfo.InvariantCulture) ?? key;
        return (english, localized == english ? null : localized);
    }

    internal static string Format(string key, params object?[] args)
        => string.Format(CultureInfo.CurrentCulture, Get(key), args);

    internal static string EnumLabel(Enum value, string? fallback = null)
    {
        var type = value.GetType();
        var name = value.ToString();
        return GetOptional($"Enum_{type.Name}_{name}")
            ?? fallback
            ?? type.GetField(name)?.GetCustomAttribute<DescriptionAttribute>()?.Description
            ?? name;
    }

    internal static string StatLabel(string id) => id switch
    {
        "ENCDPS" => Get("StatEncounterDps"),
        "ENCHPS" => Get("StatEncounterHps"),
        "DPS" => Get("StatActiveDps"),
        "Damage%" => Get("StatDamageShare"),
        "DirectHit%" => Get("StatDirectHitRate"),
        "CritHit%" => Get("StatCriticalHitRate"),
        "Deaths" => Get("StatDeaths"),
        _ => id,
    };

    internal static string PresetLabel(string name) => name switch
    {
        "Default Stylish" => Get("PresetDefaultStylish"),
        "Compact Underline" => Get("PresetCompactUnderline"),
        "Combat Table" => Get("PresetCombatTable"),
        "Party Cards" => Get("PresetPartyCards"),
        "Compact Tiles" => Get("PresetCompactTiles"),
        "Best Hit Strip" => Get("PresetBestHitStrip"),
        _ => name,
    };

    internal static string TagLabel(string label) => GetOptional("TagLabel_" + label) ?? label;

    internal static string? GetOptional(string key) => Resources.GetString(key, uiCulture);
}
