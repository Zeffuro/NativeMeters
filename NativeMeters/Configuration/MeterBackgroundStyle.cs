using System.ComponentModel;

namespace NativeMeters.Configuration;

public enum MeterBackgroundStyle
{
    [Description("Tooltip (Original)")]
    Tooltip = 0,

    [Description("Addon Window")]
    Window = 1,

    [Description("Addon Window (Bright Border)")]
    WindowFocused = 2,
}
