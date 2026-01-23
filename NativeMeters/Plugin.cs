using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiToolKit;
using KamiToolKit.Overlay;
using NativeMeters.Addons;
using NativeMeters.Clients;
using NativeMeters.Commands;
using NativeMeters.Helpers;
using NativeMeters.Services;

namespace NativeMeters;

public class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        System.Config = Util.LoadConfigOrDefault();
        BackupHelper.DoConfigBackup(pluginInterface);

        KamiToolKitLibrary.Initialize(pluginInterface);
        System.OverlayController = new OverlayController();

        System.MeterService = new MeterService(new WebSocketClient(), new IINACTIpcClient());
        System.TestMeterService = new TestMeterService();
        System.ActiveMeterService = System.MeterService;

        System.AddonConfigurationWindow = new AddonConfigurationWindow
        {
            InternalName = "NativeMeters Config",
            Title = "NativeMeters Config",
            Size = new Vector2(640, 512),
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

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.ClientState.Login -= OnLogin;

        System.OverlayController.Dispose();
        System.TestMeterService.Dispose();
        System.MeterService.Dispose();
        System.OverlayManager.Dispose();
        System.CommandHandler.Dispose();
        System.AddonConfigurationWindow.Dispose();

        Util.SaveConfig(System.Config);
        KamiToolKitLibrary.Dispose();
    }

    private void OnFrameworkUpdate(IFramework framework) {
        System.MeterService.ProcessPendingMessages();

        if (System.Config.General.PreviewEnabled) System.TestMeterService.Tick();
    }

    private void OnLogin() {
        System.MeterService.Enable();
        System.AddonConfigurationWindow.DebugOpen();
    }
}
