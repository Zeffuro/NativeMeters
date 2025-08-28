using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;

namespace NativeMeters.Services;

public class Service
{
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IPluginLog Logger { get; private set; } = null!;
    public static IMeterService ActiveMeterService { get; set; } = null!;
    public static MeterService MeterService { get; set; } = null!;
    public static TestMeterService TestMeterService { get; set; } = null!;
    public static NativeController NativeController { get; set; } = null!;
    public static NameplateAddonController NameplateAddonController { get; set; } = null!;
    public static OverlayController OverlayController { get; set; } = null!;
}