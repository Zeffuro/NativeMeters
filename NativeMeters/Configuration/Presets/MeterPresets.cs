using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;

namespace NativeMeters.Configuration.Presets;

public static class MeterPresets
{
    public static void ApplyDefaultStylish(MeterSettings settings)
    {
        settings.RowHeight = 32.0f;
        settings.RowSpacing = 0f;
        settings.HeaderHeight = 24.0f;
        settings.FooterHeight = 24.0f;
        settings.HeaderEnabled = true;
        settings.FooterEnabled = true;
        settings.Size = new Vector2(280, 320);
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
            Position = new Vector2(32, 10),
            Size = new Vector2(80, 40),
            FontSize = 16,
            TextColor = ColorHelper.GetColor(37),
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        },
        new()
        {
            Name = "Encounter Name",
            Type = MeterComponentType.Text,
            DataSource = "[name]",
            Position = new Vector2(250, 10),
            Size = new Vector2(200, 40),
            FontSize = 16,
            AlignmentType = AlignmentType.Right,
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
            Position = new Vector2(0, 2),
            Size = Vector2.Zero,
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        },
        new()
        {
            Name = "Raid DPS Label",
            Type = MeterComponentType.Text,
            DataSource = "Raid DPS: [dps]",
            Position = new Vector2(250, 2),
            Size = Vector2.Zero,
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
            Position = new Vector2(32, 10),
            Size = new Vector2(230, 20),
            ZIndex = 0,
            ColorMode = ColorMode.Job,
        },
        new()
        {
            Name = "Job Icon",
            Type = MeterComponentType.JobIcon,
            Position = new Vector2(2, 4),
            Size = new Vector2(32),
            ZIndex = 1
        },
        new()
        {
            Name = "Player Name",
            Type = MeterComponentType.Text,
            DataSource = "[name]",
            Position = new Vector2(32, 5),
            Size = new Vector2(150, 20),
            ZIndex = 2,
            FontSize = 14,
            TextFlags = TextFlags.Edge,
            ColorMode = ColorMode.Job,
            TextColor = ColorHelper.GetColor(50)
        },
        new()
        {
            Name = "Stat Value",
            Type = MeterComponentType.Text,
            DataSource = "[ENCDPS]",
            Position = new Vector2(250, 5),
            Size = new Vector2(60, 20),
            ZIndex = 2,
            FontSize = 14,
            AlignmentType = AlignmentType.Right,
            TextFlags = TextFlags.Edge,
            ColorMode = ColorMode.Static,
            TextColor = new Vector4(1, 1, 1, 0.9f)
        }
    ];
}
