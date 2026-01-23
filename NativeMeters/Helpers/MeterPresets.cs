using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Configuration;

namespace NativeMeters.Helpers;

public static class MeterPresets
{
    public static void ApplyDefaultStylish(MeterSettings settings)
    {
        settings.HeaderHeight = 24.0f;
        settings.FooterHeight = 24.0f;
        settings.HeaderEnabled = true;
        settings.FooterEnabled = true;
        settings.ShowWindowBackground = true;

        settings.HeaderComponents = GetDefaultHeaderComponents();
        settings.RowComponents = GetDefaultStylishComponents();
        settings.FooterComponents = GetDefaultFooterComponents();
    }

    public static List<ComponentSettings> GetDefaultHeaderComponents() =>
    [
        new()
        {
            Name = "Duration",
            Type = MeterComponentType.Text,
            DataSource = "[duration]",
            Position = new Vector2(5, 2),
            Size = new Vector2(40, 20),
            FontSize = 13,
            TextColor = new Vector4(0.5f, 0.9f, 1.0f, 1.0f), // Light blue
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        },
        new()
        {
            Name = "Encounter Name",
            Type = MeterComponentType.Text,
            DataSource = "[zone]",
            Position = new Vector2(45, 2),
            Size = new Vector2(150, 20),
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        }
    ];

    public static List<ComponentSettings> GetDefaultFooterComponents() =>
    [
        new()
        {
            Name = "Deaths Label",
            Type = MeterComponentType.Text,
            DataSource = "Deaths: [deaths]",
            Position = new Vector2(5, 2),
            Size = new Vector2(80, 20),
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        },
        new()
        {
            Name = "Raid DPS Label",
            Type = MeterComponentType.Text,
            DataSource = "Raid DPS: [dps]",
            Position = new Vector2(5, 2),
            Size = new Vector2(120, 20),
            FontSize = 14,
            AlignmentType = AlignmentType.Right,
            ZIndex = 1
        }
    ];

    public static List<ComponentSettings> GetDefaultStylishComponents() =>
    [
        new()
        {
            Name = "Progress Bar",
            Type = MeterComponentType.ProgressBar,
            Position = new Vector2(0, 2),
            Size = new Vector2(250, 26),
            ZIndex = 0,
            UseJobColor = true
        },
        new()
        {
            Name = "Job Icon",
            Type = MeterComponentType.JobIcon,
            Position = new Vector2(2, 4),
            Size = new Vector2(32),
            ZIndex = 2
        },
        new()
        {
            Name = "Player Name",
            Type = MeterComponentType.Text,
            DataSource = "[name]",
            Position = new Vector2(28, 5),
            Size = new Vector2(150, 20),
            ZIndex = 2,
            FontSize = 14,
            TextFlags = TextFlags.Edge,
            UseJobColor = false,
            TextColor = ColorHelper.GetColor(50)
        },
        new()
        {
            Name = "Stat Value",
            Type = MeterComponentType.Text,
            DataSource = "[ENCDPS]",
            Position = new Vector2(5, 5),
            Size = new Vector2(60, 20),
            ZIndex = 2,
            FontSize = 14,
            AlignmentType = AlignmentType.Right,
            TextFlags = TextFlags.Edge,
            UseJobColor = true,
            TextColor = new Vector4(1, 1, 1, 0.9f)
        }
    ];
}
