using System.ComponentModel;

namespace NativeMeters.Configuration;

public enum ParseFilter
{
    [Description("None (All Entities)")]
    None,
    [Description("Self (You & Pets)")]
    Self,
    [Description("Party (Party & Pets)")]
    Party,
    [Description("Alliance (Alliance & Pets)")]
    Alliance
}

public class InternalParserSettings
{
    public ParseFilter ParseFilter { get; set; } = ParseFilter.Alliance;
    public bool MergePetDamage { get; set; } = true;
    public bool UseYouForLocalPlayer { get; set; } = false;
    public bool ShowCompanions { get; set; } = true;
}
