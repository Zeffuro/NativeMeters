using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Services;

namespace NativeMeters.Configuration.Presets;

public static class MeterPresets
{
    private static string PresetsDirectory => Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "Assets", "Presets");

    public static List<string> GetPresetNames()
    {
        var names = new List<string> { "Default Stylish" };

        if (Directory.Exists(PresetsDirectory))
        {
            var files = Directory.GetFiles(PresetsDirectory, "*.txt")
                .Select(Path.GetFileNameWithoutExtension);
            names.AddRange(files!);
        }

        return names;
    }

    public static void ApplyPreset(string name, MeterSettings target)
    {
        if (name == "Default Stylish")
        {
            ApplyDefaultStylish(target);
            return;
        }

        var filePath = Path.Combine(PresetsDirectory, $"{name}.txt");
        if (File.Exists(filePath))
        {
            var blob = File.ReadAllText(filePath);
            var imported = ConfigSerializer.DeserializeCompressed<MeterSettings>(blob);
            if (imported != null)
            {
                ApplySettings(imported, target);
            }
        }
    }

    public static void ApplySettings(MeterSettings source, MeterSettings target)
    {
        var oldId = target.Id;
        var oldPos = target.Position;

        foreach (var prop in typeof(MeterSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop is { CanWrite: true, CanRead: true })
            {
                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }

        target.Id = oldId;
        target.Position = oldPos;
    }

    public static void ApplyDefaultStylish(MeterSettings settings)
    {
        settings.RowHeight = 32.0f;
        settings.RowSpacing = 0f;
        settings.HeaderHeight = 34.0f;
        settings.FooterHeight = 24.0f;
        settings.HeaderEnabled = true;
        settings.FooterEnabled = true;
        settings.Size = new Vector2(270, 320);
        settings.ShowWindowBackground = false;

        settings.HeaderComponents = GetDefaultHeaderComponents();
        settings.RowComponents = GetDefaultStylishComponents();
        settings.FooterComponents = GetDefaultFooterComponents();
    }

    public static List<ComponentSettings> GetDefaultHeaderComponents() =>
    [
        new()
        {
            Name = "Menu Button",
            Type = MeterComponentType.MenuButton,
            Position = new Vector2(6, 0),
            Size = new Vector2(24, 24),
            ZIndex = 2
        },
        new()
        {
            Name = "Duration",
            Type = MeterComponentType.Text,
            DataSource = "[duration]",
            Position = new Vector2(32, 2),
            Size = new Vector2(60, 20),
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
            Position = new Vector2(88, 2),
            Size = new Vector2(170, 20),
            FontSize = 16,
            AlignmentType = AlignmentType.Right,
            ZIndex = 1
        },
        new ()
        {
            Name = "Separator",
            Type = MeterComponentType.Separator,
            Position = new Vector2(8, 25),
            Size = new Vector2(254, 4),
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
            Size = new Vector2(270, 20),
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            ZIndex = 1
        },
        new()
        {
            Name = "Raid DPS Label",
            Type = MeterComponentType.Text,
            DataSource = "Raid DPS: [dps]",
            Position = new Vector2(-14, 2),
            Size = new Vector2(270, 20),
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
            Position = new Vector2(2, 0),
            Size = new Vector2(32),
            ZIndex = 1
        },
        new()
        {
            Name = "Player Name",
            Type = MeterComponentType.Text,
            DataSource = "[name]",
            Position = new Vector2(36, -5),
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
            Position = new Vector2(26, -5),
            Size = new Vector2(230, 20),
            ZIndex = 2,
            FontSize = 14,
            AlignmentType = AlignmentType.Right,
            TextFlags = TextFlags.Edge,
            ColorMode = ColorMode.Static,
            TextColor = new Vector4(1, 1, 1, 0.9f)
        }
    ];
}
