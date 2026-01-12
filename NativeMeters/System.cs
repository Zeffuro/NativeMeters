using KamiToolKit.Overlay;
using NativeMeters.Addons;
using NativeMeters.Configuration;
using NativeMeters.Services;

namespace NativeMeters;

public static class System
{
    public static IMeterService ActiveMeterService { get; set; } = null!;
    public static AddonConfigurationWindow AddonConfigurationWindow { get; set; } = null!;
    public static SystemConfiguration Config { get; set; } = null!;
    public static MeterService MeterService { get; set; } = null!;
    public static OverlayController OverlayController { get; set; } = null!;
    public static OverlayManager OverlayManager { get; set; } = null!;
    public static TestMeterService TestMeterService { get; set; } = null!;
}