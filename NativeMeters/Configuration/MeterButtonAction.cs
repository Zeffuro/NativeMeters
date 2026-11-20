using System.ComponentModel;
using NativeMeters.Localization;

namespace NativeMeters.Configuration;

public enum MeterButtonAction
{
    [Description("Context Menu")]
    ContextMenu = 0,

    [Description("Collapse / Expand")]
    ToggleCollapse = 1,

    [Description("Reset All Dummies")]
    ResetDummies = 2,

    [Description("Detailed Breakdown")]
    DetailedBreakdown = 3,

    Settings = 4,

    [Description("Copy Encounter Summary")]
    CopyEncounterSummary = 5,
}

public static class MeterComponentTypeExtensions
{
    public static string GetDisplayName(this MeterComponentType type)
        => Loc.EnumLabel(type, type == MeterComponentType.MenuButton ? "Button" : type.ToString());
}
