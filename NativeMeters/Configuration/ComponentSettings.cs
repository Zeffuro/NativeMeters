using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Data.Stats;
using NativeMeters.Models;

namespace NativeMeters.Configuration;

public enum MeterComponentType { Text = 0, JobIcon = 1, ProgressBar = 2, Background = 3, MenuButton = 4, Separator = 5, Icon = 6, Asset = 7 }
public enum ColorMode { Static, Job, Role }
public enum ProgressBarSizing
{
    Stretch,

    [Description("Preserve ends")]
    PreserveEnds,
}

public enum ProgressBarColorTreatment
{
    Auto,

    [Description("Flat Color")]
    Flat,

    [Description("Native Tint")]
    NativeTint,

    [Description("Legacy Additive")]
    LegacyAdditive,

    [Description("Stock Colors")]
    StockColors,
}

public class ComponentSettings
{
    public const string DefaultTextDataSource = "[name]";

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MeterComponentType Type { get; set; }
    public string Name { get; set; } = "New Component";

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = new(100, 20);
    public int ZIndex { get; set; } = 0;

    public string DataSource { get; set; } = DefaultTextDataSource;

    public JobIconType JobIconType { get; set; } = JobIconType.Default;
    public uint IconId { get; set; } = 0;
    public MeterAsset Asset { get; set; } = MeterAsset.DutyClock;
    public MeterButtonAction ButtonAction { get; set; } = MeterButtonAction.ContextMenu;

    public uint FontSize { get; set; } = 14;
    public FontType FontType { get; set; } = FontType.Axis;
    public TextFlags TextFlags { get; set; } = TextFlags.Edge;
    public AlignmentType AlignmentType { get; set; } = AlignmentType.Left;
    public Vector4 TextColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 TextOutlineColor { get; set; } = ColorHelper.GetColor(51);
    public Vector4 TextBackgroundColor { get; set; } = KnownColor.Black.Vector();
    public Vector4 BarColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 BarBackgroundColor { get; set; } = KnownColor.Black.Vector();
    public ColorMode ColorMode { get; set; } = ColorMode.Job;
    public ProgressBarType ProgressBarType { get; set; } = ProgressBarType.Cast;
    public ProgressBarSizing ProgressBarSizing { get; set; }
    public ProgressBarColorTreatment ProgressBarColorTreatment { get; set; } = ProgressBarColorTreatment.Auto;
    public bool ProgressBarFillRightToLeft { get; set; }
    public bool ShowBackground { get; set; }

    internal bool PreserveBarEnds => ProgressBarSizing == ProgressBarSizing.PreserveEnds &&
        ProgressBarTypeOptions.SupportsPreservingEnds(ProgressBarType);

    public void EnsureInitialized()
    {
        if (Type == MeterComponentType.ProgressBar)
            DataSource = StatSelector.NormalizeStatSelector(DataSource);
    }

    public ComponentSettings DeepCopy()
    {
        var clone = (ComponentSettings) MemberwiseClone();

        clone.Id = Guid.NewGuid().ToString();
        clone.Name = Name + " (Copy)";
        clone.Position = Position + new Vector2(10, 10);

        return clone;
    }
}
