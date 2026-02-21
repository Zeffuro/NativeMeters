using KamiToolKit.Overlay;
using NativeMeters.Addons;
using NativeMeters.Commands;
using NativeMeters.Configuration;
using NativeMeters.Services;
using NativeMeters.Services.Internal;

namespace NativeMeters;

public static class System
{
    public static IMeterService ActiveMeterService { get; set; } = null!;
    public static AddonConfigurationWindow AddonConfigurationWindow { get; set; } = null!;
    public static AddonDetailedBreakdownWindow AddonDetailedBreakdownWindow { get; set; } = null!;
    public static SystemConfiguration Config { get; set; } = null!;
    public static CommandHandler CommandHandler { get; set; } = null!;
    public static DtrService DtrService { get; set; } = null!;
    public static MeterService MeterService { get; set; } = null!;
    public static InternalMeterService InternalMeterService { get; set; } = null!;
    public static OverlayController OverlayController { get; set; } = null!;
    public static OverlayManager OverlayManager { get; set; } = null!;
    public static TestMeterService TestMeterService { get; set; } = null!;
}
