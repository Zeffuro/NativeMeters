using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NativeMeters.Configuration;

public class MeterSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New Meter";

    public bool IsEnabled { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public bool IsClickthrough { get; set; } = false;
    public bool IsCollapsed { get; set; } = false;

    public Vector2 Position { get; set; } = new(500, 500);
    public Vector2 Size { get; set; } = new(250, 300);
    public int Scale { get; set; } = 100;

    public float RowHeight { get; set; } = 36.0f;
    public float RowSpacing { get; set; } = 0.0f;

    public bool HeaderEnabled { get; set; } = true;
    public bool FooterEnabled { get; set; } = true;
    public float HeaderHeight { get; set; } = 28.0f;
    public float FooterHeight { get; set; } = 28.0f;

    public int MaxCombatants { get; set; } = 8;
    public bool ShowLimitBreak { get; set; } = true;
    public bool ShowNonPlayerCombatants { get; set; } = false;
    public bool PinSelfToTop { get; set; } = false;
    public string StatToTrack { get; set; } = "ENCDPS";
    public ProgressBarType ProgressBarType { get; set; } = ProgressBarType.Cast;
    public bool ShowWindowBackground { get; set; } = true;
    public Vector4 WindowColor { get; set; } = new(0, 0, 0, 0.5f);
    public SelfRowOverrideSettings SelfRowOverride { get; set; } = new();

    public List<ComponentSettings> RowComponents { get; set; } = [];
    public List<ComponentSettings> HeaderComponents { get; set; } = [];
    public List<ComponentSettings> FooterComponents { get; set; } = [];

    public void EnsureInitialized()
    {
        SelfRowOverride ??= new SelfRowOverrideSettings();
        RowComponents ??= [];
        HeaderComponents ??= [];
        FooterComponents ??= [];
    }
}

public class SelfRowOverrideSettings
{
    public bool Enabled { get; set; } = false;

    public bool OverrideTextColor { get; set; } = true;
    public SelfTextOverrideMode TextColorMode { get; set; } = SelfTextOverrideMode.NameOnly;
    public Vector4 TextColor { get; set; } = new(200f / 255f, 1.0f, 70f / 255f, 1.0f);

    public bool OverrideBarColor { get; set; } = true;
    public Vector4 BarColor { get; set; } = new(200f / 255f, 1.0f, 70f / 255f, 1.0f);

    public bool OverrideTextOutlineColor { get; set; } = false;
    public Vector4 TextOutlineColor { get; set; } = new(0, 0, 0, 1.0f);

    public bool OverrideTextBackgroundColor { get; set; } = false;
    public Vector4 TextBackgroundColor { get; set; } = new(0, 0, 0, 0.5f);

    public bool OverrideTextStyle { get; set; } = false;
    public uint TextFontSize { get; set; } = 14;
    public FontType TextFontType { get; set; } = FontType.Axis;
    public TextFlags TextFlags { get; set; } = TextFlags.Edge;

    public bool OverrideBarBackgroundColor { get; set; } = false;
    public Vector4 BarBackgroundColor { get; set; } = new(0, 0, 0, 0.5f);

    public bool OverrideRowBackgroundColor { get; set; } = false;
    public Vector4 RowBackgroundColor { get; set; } = new(1.0f, 0.85f, 0.25f, 0.25f);
}

public enum SelfTextOverrideMode
{
    [Description("Name Only")]
    NameOnly,
    [Description("All Row Text")]
    AllText
}

public enum ProgressBarType
{
    Cast,
    EnemyCast,
    ToDo
}
