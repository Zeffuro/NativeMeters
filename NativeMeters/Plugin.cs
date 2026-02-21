using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;
using KamiToolKit.Overlay;
using NativeMeters.Addons;
using NativeMeters.Clients;
using NativeMeters.Commands;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;
using NativeMeters.Nodes.Color;
using NativeMeters.Services;
using NativeMeters.Services.Internal;

namespace NativeMeters;

public class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        System.Config = ConfigRepository.LoadOrDefault();
        ConfigBackup.DoConfigBackup(pluginInterface);

        KamiToolKitLibrary.Initialize(pluginInterface);
        System.OverlayController = new OverlayController();

        System.MeterService = new MeterService(new WebSocketClient(), new IINACTIpcClient());
        System.InternalMeterService = new InternalMeterService();
        System.TestMeterService = new TestMeterService();
        System.ActiveMeterService = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal
            ? System.InternalMeterService
            : System.MeterService;

        System.DtrService = new DtrService();

        System.AddonConfigurationWindow = new AddonConfigurationWindow
        {
            InternalName = "NativeMeters_Config",
            Title = "NativeMeters Config",
            Size = new Vector2(640, 512),
        };

        System.AddonDetailedBreakdownWindow = new AddonDetailedBreakdownWindow
        {
            InternalName = "NativeMeters_Breakdown",
            Title = "NativeMeters Breakdown",
            Size = new Vector2(580, 480),
        };

        Service.PluginInterface.UiBuilder.OpenMainUi += System.AddonConfigurationWindow.Toggle;
        Service.PluginInterface.UiBuilder.OpenConfigUi += System.AddonConfigurationWindow.Toggle;

        System.CommandHandler = new CommandHandler();

        System.OverlayManager = new OverlayManager();
        System.OverlayManager.Setup();

        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.Login += OnLogin;

        if (Service.ClientState.IsLoggedIn) {
            Service.Framework.RunOnFrameworkThread(OnLogin);
        }
    }

    private void OnFrameworkUpdate(IFramework framework) {
        System.MeterService.ProcessPendingMessages();

        if (System.Config.General.PreviewEnabled) System.TestMeterService.Tick();
    }

    private void OnLogin() {
        if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
        {
            System.InternalMeterService.Enable();
        }
        else
        {
            System.MeterService.Enable();

            if (System.Config.General.EnableInternalParserForBreakdown)
            {
                System.InternalMeterService.Enable();
            }
        }
        System.AddonConfigurationWindow.DebugOpen();
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.ClientState.Login -= OnLogin;

        ColorInputRow.DisposeSharedColorPicker();

        System.OverlayController.Dispose();
        System.DtrService.Dispose();
        System.TestMeterService.Dispose();
        System.InternalMeterService.Dispose();
        System.MeterService.Dispose();
        System.OverlayManager.Dispose();
        System.CommandHandler.Dispose();
        System.AddonConfigurationWindow.Dispose();
        System.AddonDetailedBreakdownWindow.Dispose();

        ConfigRepository.Save(System.Config);
        KamiToolKitLibrary.Dispose();
    }
}
