using System.ComponentModel;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;

namespace NativeMeters.Configuration;

public class PartyListMeterSettings
{
    private const int CurrentSettingsVersion = 1;

    public int SettingsVersion { get; set; }

    public bool Enabled { get; set; } = false;

    public bool ShowSelf { get; set; } = true;
    public bool ShowPartyMembers { get; set; } = true;
    public bool HideWhenNoCombatData { get; set; } = true;

    public string MemberFormat { get; set; } = "[dps:c.1]";

    public bool ShowMemberBars { get; set; } = false;

    public bool ShowRaidDps { get; set; } = true;
    public string RaidDpsFormat { get; set; } = "[encdps:c.1]";

    public PartyListMeterAnchor Anchor { get; set; } = PartyListMeterAnchor.MemberRow;
    public int OffsetX { get; set; } = -38;
    public int OffsetY { get; set; } = 25;
    public int Width { get; set; } = 54;
    public int Height { get; set; } = 22;

    public int BarOffsetX { get; set; } = -16;
    public int BarOffsetY { get; set; } = 42;
    public int BarWidth { get; set; } = 32;
    public int BarHeight { get; set; } = 8;
    public ProgressBarType BarType { get; set; } = ProgressBarType.PartyListHp;
    public bool BarFillRightToLeft { get; set; } = true;
    public ColorMode BarColorMode { get; set; } = ColorMode.Static;
    public ProgressBarColorTreatment BarColorTreatment { get; set; } = ProgressBarColorTreatment.Flat;

    public int RaidOffsetX { get; set; } = 124;
    public int RaidOffsetY { get; set; } = 0;
    public int RaidWidth { get; set; } = 88;
    public int RaidHeight { get; set; } = 22;

    public uint FontSize { get; set; } = 10;
    public FontType FontType { get; set; } = FontType.MiedingerMed;
    public TextFlags TextFlags { get; set; } = TextFlags.Edge;
    public AlignmentType MemberAlignment { get; set; } = AlignmentType.Right;
    public AlignmentType RaidAlignment { get; set; } = AlignmentType.Left;

    public Vector4 TextColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 TextOutlineColor { get; set; } = ColorHelper.GetColor(36);
    public Vector4 BarColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 BarBackgroundColor { get; set; } = ColorHelper.GetColor(50);

    public void ResetToDefaults()
    {
        var defaults = new PartyListMeterSettings();

        SettingsVersion = CurrentSettingsVersion;
        Enabled = defaults.Enabled;
        ShowSelf = defaults.ShowSelf;
        ShowPartyMembers = defaults.ShowPartyMembers;
        HideWhenNoCombatData = defaults.HideWhenNoCombatData;
        MemberFormat = defaults.MemberFormat;
        ShowMemberBars = defaults.ShowMemberBars;
        ShowRaidDps = defaults.ShowRaidDps;
        RaidDpsFormat = defaults.RaidDpsFormat;
        Anchor = defaults.Anchor;
        OffsetX = defaults.OffsetX;
        OffsetY = defaults.OffsetY;
        Width = defaults.Width;
        Height = defaults.Height;
        BarOffsetX = defaults.BarOffsetX;
        BarOffsetY = defaults.BarOffsetY;
        BarWidth = defaults.BarWidth;
        BarHeight = defaults.BarHeight;
        BarType = defaults.BarType;
        BarFillRightToLeft = defaults.BarFillRightToLeft;
        BarColorMode = defaults.BarColorMode;
        BarColorTreatment = defaults.BarColorTreatment;
        RaidOffsetX = defaults.RaidOffsetX;
        RaidOffsetY = defaults.RaidOffsetY;
        RaidWidth = defaults.RaidWidth;
        RaidHeight = defaults.RaidHeight;
        FontSize = defaults.FontSize;
        FontType = defaults.FontType;
        TextFlags = defaults.TextFlags;
        MemberAlignment = defaults.MemberAlignment;
        RaidAlignment = defaults.RaidAlignment;
        TextColor = defaults.TextColor;
        TextOutlineColor = defaults.TextOutlineColor;
        BarColor = defaults.BarColor;
        BarBackgroundColor = defaults.BarBackgroundColor;
    }

    public void EnsureInitialized()
    {
        var defaults = new PartyListMeterSettings();

        MemberFormat ??= defaults.MemberFormat;
        RaidDpsFormat ??= defaults.RaidDpsFormat;

        SettingsVersion = CurrentSettingsVersion;
    }
}

public enum PartyListMeterAnchor
{
    [Description("HP Bar")]
    HpBar,

    [Description("MP Bar")]
    MpBar,

    [Description("Name/Bars")]
    NameAndBars,

    [Description("Member Row")]
    MemberRow
}
