using System.Collections.Generic;
using System.Numerics;
using NativeMeters.Configuration;

namespace NativeMeters.Helpers;

public static class MeterPresets
{
    public static List<ComponentSettings> GetDefaultStylishComponents() =>
    [
        new()
        {
            Name = "Job Icon",
            Type = MeterComponentType.JobIcon,
            Position = new Vector2(0, 2),
            Size = new Vector2(32, 32),
            ZIndex = 1
        },

        new()
        {
            Name = "Progress Bar",
            Type = MeterComponentType.ProgressBar,
            Position = new Vector2(32, 10),
            Size = new Vector2(200, 20),
            ZIndex = 0
        },

        new()
        {
            Name = "Player Name",
            Type = MeterComponentType.Text,
            DataSource = "Name",
            Position = new Vector2(34, 5),
            Size = new Vector2(150, 20),
            ZIndex = 2,
            FontSize = 14
        },
        new()
        {
            Name = "Stat Value",
            Type = MeterComponentType.Text,
            DataSource = "ENCDPS",
            Position = new Vector2(180, 22),
            Size = new Vector2(60, 20),
            ZIndex = 2,
            FontSize = 14
        }
    ];
}