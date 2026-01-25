using System;
using System.Numerics;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;

namespace NativeMeters.Rendering;

public static class ColorResolver
{
    public static Vector4 GetColor(Combatant combatant, ColorMode mode, ComponentSettings? settings = null)
    {
        var config = System.Config.General;

        if (combatant.IsLimitBreak)
            return config.OtherColor;

        return mode switch
        {
            ColorMode.Static => settings?.BarColor ?? config.OtherColor,
            ColorMode.Role => GetRoleColor(combatant.Job.Role, config),
            ColorMode.Job => GetJobColor(combatant.Job.RowId, config),
            _ => config.OtherColor
        };
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
}
