using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;

namespace NativeMeters.Rendering;

public static class ColorResolver
{
    public static Vector4 GetColor(
        Combatant combatant,
        ColorMode mode,
        ComponentSettings? settings = null,
        MeterSettings? meterSettings = null)
    {
        var config = System.Config.General;

        if (combatant.IsLimitBreak)
            return config.OtherColor;

        if (settings != null && TryGetSelfForegroundColor(combatant, settings, meterSettings, out var overrideColor))
            return overrideColor;

        return mode switch
        {
            ColorMode.Static => GetStaticColor(settings, config),
            ColorMode.Role => GetRoleColor(combatant.Job.Role, config),
            ColorMode.Job => GetJobColor(combatant.Job.RowId, config),
            _ => config.OtherColor
        };
    }

    public static Vector4 GetTextOutlineColor(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideTextOutlineColor
            && IsTextOverrideTarget(settings, selfOverride))
        {
            return selfOverride.TextOutlineColor;
        }

        return settings.TextOutlineColor;
    }

    public static Vector4 GetTextBackgroundColor(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideTextBackgroundColor
            && IsTextOverrideTarget(settings, selfOverride))
        {
            return selfOverride.TextBackgroundColor;
        }

        return settings.TextBackgroundColor;
    }

    public static uint GetTextFontSize(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideTextStyle
            && IsTextOverrideTarget(settings, selfOverride))
        {
            return selfOverride.TextFontSize;
        }

        return settings.FontSize;
    }

    public static FontType GetTextFontType(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideTextStyle
            && IsTextOverrideTarget(settings, selfOverride))
        {
            return selfOverride.TextFontType;
        }

        return settings.FontType;
    }

    public static TextFlags GetTextFlags(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideTextStyle
            && IsTextOverrideTarget(settings, selfOverride))
        {
            return selfOverride.TextFlags;
        }

        return settings.TextFlags;
    }

    public static Vector4 GetBarBackgroundColor(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideBarBackgroundColor)
        {
            return selfOverride.BarBackgroundColor;
        }

        return settings.BarBackgroundColor;
    }

    public static Vector4 GetRowBackgroundColor(Combatant combatant, ComponentSettings settings, MeterSettings? meterSettings = null)
    {
        if (TryGetSelfOverride(combatant, meterSettings, out var selfOverride)
            && selfOverride.OverrideRowBackgroundColor)
        {
            return selfOverride.RowBackgroundColor;
        }

        return settings.TextColor;
    }

    public static Vector4 GetDefaultColor(Combatant combatant)
    {
        if (combatant.IsLimitBreak)
            return JobColorMaps.DefaultColors[0];

        if (combatant.Job.RowId != 0 && JobColorMaps.DefaultColors.TryGetValue(combatant.Job.RowId, out var color))
            return color;

        return JobColorMaps.DefaultColors[0];
    }

    private static Vector4 GetRoleColor(byte role, GeneralSettings config) => role switch
    {
        1 => config.TankColor,
        4 => config.HealerColor,
        2 or 3 => config.DpsColor,
        _ => config.OtherColor
    };

    private static Vector4 GetJobColor(uint jobId, GeneralSettings config)
    {
        if (config.JobColors.TryGetValue(jobId, out var jobColor))
            return jobColor;

        return config.OtherColor;
    }

    private static Vector4 GetStaticColor(ComponentSettings? settings, GeneralSettings config)
    {
        if (settings == null) return config.OtherColor;

        return settings.Type switch
        {
            MeterComponentType.Text or MeterComponentType.Background => settings.TextColor,
            MeterComponentType.ProgressBar => settings.BarColor,
            _ => settings.BarColor
        };
    }

    private static bool TryGetSelfForegroundColor(
        Combatant combatant,
        ComponentSettings settings,
        MeterSettings? meterSettings,
        out Vector4 color)
    {
        color = default;

        if (!TryGetSelfOverride(combatant, meterSettings, out var selfOverride))
            return false;

        switch (settings.Type)
        {
            case MeterComponentType.Text when selfOverride.OverrideTextColor && IsTextOverrideTarget(settings, selfOverride):
                color = selfOverride.TextColor;
                return true;

            case MeterComponentType.ProgressBar when selfOverride.OverrideBarColor:
                color = selfOverride.BarColor;
                return true;

            case MeterComponentType.Background when selfOverride.OverrideRowBackgroundColor:
                color = selfOverride.RowBackgroundColor;
                return true;

            default:
                return false;
        }
    }

    private static bool TryGetSelfOverride(
        Combatant combatant,
        MeterSettings? meterSettings,
        out SelfRowOverrideSettings selfOverride)
    {
        selfOverride = null!;

        if (combatant.IsLimitBreak)
            return false;

        if (meterSettings?.SelfRowOverride is not { Enabled: true } settings)
            return false;

        if (!combatant.IsSelf)
            return false;

        selfOverride = settings;
        return true;
    }

    private static bool IsTextOverrideTarget(ComponentSettings settings, SelfRowOverrideSettings selfOverride)
    {
        if (settings.Type != MeterComponentType.Text)
            return false;

        return selfOverride.TextColorMode switch
        {
            SelfTextOverrideMode.AllText => true,
            SelfTextOverrideMode.NameOnly => IsNameTextComponent(settings),
            _ => false
        };
    }

    private static bool IsNameTextComponent(ComponentSettings settings)
        => settings.DataSource?.Contains("[name", global::System.StringComparison.OrdinalIgnoreCase) == true;
}
