using System.ComponentModel;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Data.Stats;

namespace NativeMeters.Configuration;

public class PartyListMeterSettings
{
    private const int CurrentSettingsVersion = 3;

    public int SettingsVersion { get; set; }

    public bool Enabled { get; set; } = false;

    public bool ShowSelf { get; set; } = true;
    public bool ShowPartyMembers { get; set; } = true;
    public bool HideWhenNoCombatData { get; set; } = true;

    public string MemberFormat { get; set; } = "[dps:c.1]";

    public bool HighlightTopMember { get; set; } = false;
    public string TopMemberStat { get; set; } = StatSelector.DefaultStatSelector;
    public string TopMemberFormat { get; set; } = "[dps:c.1]";

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
    public uint RaidFontSize { get; set; } = 10;
    public FontType RaidFontType { get; set; } = FontType.MiedingerMed;
    public TextFlags RaidTextFlags { get; set; } = TextFlags.Edge;

    public Vector4 TextColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 TopMemberTextColor { get; set; } = new(1.0f, 0.84f, 0.22f, 1.0f);
    public Vector4 TextOutlineColor { get; set; } = ColorHelper.GetColor(36);
    public Vector4 RaidTextColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 RaidTextOutlineColor { get; set; } = ColorHelper.GetColor(36);
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
        HighlightTopMember = defaults.HighlightTopMember;
        TopMemberStat = defaults.TopMemberStat;
        TopMemberFormat = defaults.TopMemberFormat;
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
        RaidFontSize = defaults.RaidFontSize;
        RaidFontType = defaults.RaidFontType;
        RaidTextFlags = defaults.RaidTextFlags;
        TextColor = defaults.TextColor;
        TopMemberTextColor = defaults.TopMemberTextColor;
        TextOutlineColor = defaults.TextOutlineColor;
        RaidTextColor = defaults.RaidTextColor;
        RaidTextOutlineColor = defaults.RaidTextOutlineColor;
        BarColor = defaults.BarColor;
        BarBackgroundColor = defaults.BarBackgroundColor;
    }

    public void EnsureInitialized()
    {
        var defaults = new PartyListMeterSettings();

        MemberFormat ??= defaults.MemberFormat;
        TopMemberStat = StatSelector.NormalizeStatSelector(TopMemberStat);
        TopMemberFormat ??= defaults.TopMemberFormat;
        RaidDpsFormat ??= defaults.RaidDpsFormat;

        if (SettingsVersion < 2)
        {
            TopMemberFormat = MemberFormat;
            TopMemberTextColor = TextColor;
        }

        if (SettingsVersion < 3)
        {
            RaidFontSize = FontSize;
            RaidFontType = FontType;
            RaidTextFlags = TextFlags;
            RaidTextColor = TextColor;
            RaidTextOutlineColor = TextOutlineColor;
        }

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
