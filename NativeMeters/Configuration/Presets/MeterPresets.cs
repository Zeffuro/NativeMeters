using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Data.Stats;
using NativeMeters.Localization;
using NativeMeters.Models;
using NativeMeters.Rendering;
using NativeMeters.Services;
using static NativeMeters.Configuration.Presets.MeterPresetComponents;

namespace NativeMeters.Configuration.Presets;

public static class MeterPresets
{
    private const float StylishWidth = 270f;
    private const float StylishLeft = 8f;
    private const float StylishTextLeft = 40f;
    private const float StylishRight = StylishWidth - StylishLeft;

    private static readonly PropertyInfo[] SettingsProperties = typeof(MeterSettings)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property is { CanRead: true, CanWrite: true })
        .ToArray();

    private static string PresetsDirectory => Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, "Assets", "Presets");

    public static List<string> GetPresetNames()
    {
        var names = new List<string> { "Default Stylish" };
        names.AddRange(ShowcaseMeterPresets.Names);

        if (Directory.Exists(PresetsDirectory))
        {
            var files = Directory.GetFiles(PresetsDirectory, "*.txt")
                .Select(Path.GetFileNameWithoutExtension);
            names.AddRange(files!);
        }

        return names;
    }

    public static void ApplyPreset(string name, MeterSettings target)
        => ApplyPreset(name, target, null);

    public static void ApplyPreset(string name, MeterSettings target, CustomMeterBarStyle? barStyle,
        bool? showBackground = null, ProgressBarType? barType = null, ColorMode? playerColor = null,
        ProgressBarColorTreatment? barTint = null, ColorMode? barColor = null, string? sortBy = null,
        string? fillStat = null, JobIconType? iconStyle = null, ProgressBarSizing? barSizing = null)
    {
        if (!TryApplyPreset(name, target)) return;
        ApplyAppearance(target, barStyle, showBackground, barType, playerColor, barTint, barColor, sortBy, fillStat,
            iconStyle, barSizing);
    }

    private static bool TryApplyPreset(string name, MeterSettings target)
    {
        if (name == "Default Stylish")
        {
            ApplyDefaultStylish(target);
            return true;
        }

        if (ShowcaseMeterPresets.TryApply(name, target)) return true;

        var filePath = Path.Combine(PresetsDirectory, $"{name}.txt");
        if (File.Exists(filePath))
        {
            var blob = File.ReadAllText(filePath);
            var imported = ConfigSerializer.DeserializeCompressed<MeterSettings>(blob);
            if (imported != null)
            {
                ApplySettings(imported, target);
                if (name == "FFXIV Party") AlignFfxivPartyName(target);
                return true;
            }
        }

        return false;
    }

    private static void AlignFfxivPartyName(MeterSettings target)
    {
        var bar = target.RowComponents.Find(row => row is
            { Type: MeterComponentType.ProgressBar, ProgressBarType: ProgressBarType.Cast });
        var name = target.RowComponents.Find(row => row is { Type: MeterComponentType.Text, Name: "Player Name" });
        if (bar == null || name == null) return;

        var layoutWidth = bar.PreserveBarEnds
            ? GaugeGeometry.GetUnscaledWidth(bar.Size, GaugeGeometry.CastHeight, GaugeGeometry.CastTrackCapWidth * 2)
            : GaugeGeometry.CastWidth;
        name.Position = name.Position with { X = MathF.Round(bar.Position.X + GaugeGeometry.CastFillInset * bar.Size.X / layoutWidth) };
    }

    public static void ApplySettings(MeterSettings source, MeterSettings target)
    {
        source.EnsureInitialized();

        var oldId = target.Id;
        var oldPos = target.Position;

        foreach (var property in SettingsProperties)
            property.SetValue(target, property.GetValue(source));

        target.Id = oldId;
        target.Position = oldPos;
        target.EnsureInitialized();
    }

    public static void ApplyDefaultStylish(MeterSettings settings)
    {
        settings.Layout = MeterLayout.Vertical;
        settings.RowHeight = 32.0f;
        settings.RowSpacing = 0f;
        settings.HeaderHeight = 34.0f;
        settings.FooterHeight = 24.0f;
        settings.HeaderEnabled = true;
        settings.FooterEnabled = true;
        settings.Size = new Vector2(StylishWidth, 320);
        settings.ShowWindowBackground = false;

        settings.HeaderComponents = GetDefaultHeaderComponents();
        settings.RowComponents = GetDefaultStylishComponents();
        settings.FooterComponents = GetDefaultFooterComponents();
        settings.EnsureInitialized();
    }

    public static List<ComponentSettings> GetDefaultHeaderComponents() =>
    [
        new()
        {
            Name = "Menu Button",
            Type = MeterComponentType.MenuButton,
            Position = new Vector2(StylishLeft, 2),
            Size = new Vector2(22),
            ZIndex = 2,
        },
        Text("Duration", "[duration]", StylishTextLeft, 3, 56, 20, 16, color: ColorHelper.GetColor(37)),
        Text("Encounter Name", "[name]", 102, 3, StylishRight - 102, 20, 16, AlignmentType.Right),
        new()
        {
            Name = "Separator",
            Type = MeterComponentType.Separator,
            Position = new Vector2(StylishLeft, 27),
            Size = new Vector2(StylishRight - StylishLeft, 4),
        }
    ];

    public static List<ComponentSettings> GetDefaultFooterComponents() =>
    [
        Text("Deaths Label", Loc.Format("PresetFooterDeaths", "[deaths]"), StylishLeft, 2, 94, 20),
        Text("Raid DPS Label", Loc.Format("PresetFooterRaidDps", "[dps]"), 110, 2, StylishRight - 110, 20, 14, AlignmentType.Right),
    ];

    public static List<ComponentSettings> GetDefaultStylishComponents()
    {
        const float barWidth = StylishRight - StylishTextLeft + 2 * GaugeGeometry.CastFillInset;
        const float barLeft = StylishTextLeft - GaugeGeometry.CastFillInset;
        return
        [
            new()
            {
                Name = "Progress Bar",
                Type = MeterComponentType.ProgressBar,
                DataSource = StatSelector.DefaultStatSelector,
                Position = new Vector2(barLeft, 12),
                Size = new Vector2(barWidth, GaugeGeometry.CastHeight),
                ZIndex = 0,
                ColorMode = ColorMode.Job,
                ProgressBarSizing = ProgressBarSizing.PreserveEnds,
                ProgressBarColorTreatment = ProgressBarColorTreatment.NativeTint,
                BarBackgroundColor = new Vector4(0, 0, 0, 0.38f),
            },
            Job(StylishLeft, 3, 26),
            Text("Player Name", "[name]", StylishTextLeft, 0, 148, 18),
            Text("Stat Value", "[ENCDPS]", 196, 0, StylishRight - 196, 18, 14, AlignmentType.Right),
        ];
    }
}
