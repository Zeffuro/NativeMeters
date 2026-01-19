using System.Numerics;

namespace NativeMeters.Configuration;

public class GeneralSettings
{
    public bool DebugEnabled { get; set; } = false;
    public bool HideWithNativeUi { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool PreviewEnabled { get; set; }
    public bool ReplaceYou { get; set; } = false;
}