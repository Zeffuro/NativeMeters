using System;
using NativeMeters.Localization;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.General;

internal static class LanguageDropdown
{
    private const string UseGameLanguage = "Game";

    internal static LabeledDropdownNode Create(Action<string?> changeLanguage)
        => new()
        {
            Height = 28,
            LabelText = Loc.Get("GeneralLanguage"),
            GetOptionLabel = GetLabel,
            Options = [UseGameLanguage, .. Loc.LanguageChoices],
            SelectedOption = Loc.LanguageOverride ?? UseGameLanguage,
            OnOptionSelected = choice => changeLanguage(choice == UseGameLanguage ? null : choice),
        };

    private static string GetLabel(string language) => language switch
    {
        "English" => "English",
        "Japanese" => "日本語",
        "German" => "Deutsch",
        "French" => "Français",
        "Korean" => "한국어 (KO)",
        "ChineseSimplified" => "简体中文 (CN)",
        "TraditionalChinese" => "繁體中文 (TC)",
        _ => Loc.Get("GeneralLanguageGame"),
    };
}
