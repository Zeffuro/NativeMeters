using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;
using NativeMeters.Addons;
using NativeMeters.Clients;
using NativeMeters.Commands;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Configuration.Meter.Search;
using NativeMeters.Nodes.Configuration.Meter.Tags;
using NativeMeters.Services;
using NativeMeters.Services.Internal;

namespace NativeMeters;

public class Plugin : IAsyncDalamudPlugin
{
    private bool hasHandledLogin;
    private bool isDisposed;

    [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PluginInterface.Create<Service>();
        System.Config = ConfigRepository.LoadOrDefault();
        ConfigRepository.Save(System.Config);
        ConfigBackup.DoConfigBackup(Service.PluginInterface);

        KamiToolKitLibrary.Initialize(Service.PluginInterface);

        System.MeterService = new MeterService(new WebSocketClient(), new IINACTIpcClient());
        System.InternalMeterService = new InternalMeterService();
        System.TestMeterService = new TestMeterService();
        System.ActiveMeterService = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal
            ? System.InternalMeterService
            : System.MeterService;

        System.DtrService = new DtrService();

        System.TagSearchAddon = new TagSearchAddon { Title = "Search Tags", InternalName = "NativeMeters_TagPicker", Size = new Vector2(480, 600) };
        System.IconSearchAddon = new IconSearchAddon { Title = "Select Icon", InternalName = "NativeMeters_IconPicker" };

        System.AddonConfigurationWindow = new AddonConfigurationWindow
        {
            InternalName = "NativeMeters_Config",
            Title = "NativeMeters Config",
            Size = new Vector2(800, 600),
        };

        System.AddonDetailedBreakdownWindow = new AddonDetailedBreakdownWindow
        {
            InternalName = "NativeMeters_Breakdown",
            Title = "NativeMeters Breakdown",
            Size = new Vector2(720, 560),
        };

        Service.PluginInterface.UiBuilder.OpenMainUi += System.AddonConfigurationWindow.Toggle;
        Service.PluginInterface.UiBuilder.OpenConfigUi += System.AddonConfigurationWindow.Toggle;

        System.CommandHandler = new CommandHandler();

        System.OverlayManager = new OverlayManager();
        System.OverlayManager.Setup();

        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.Login += OnLogin;

        if (Service.ClientState.IsLoggedIn) {
            try {
                Service.Framework.RunOnFrameworkThread(OnLogin);
            }
            catch (Exception exception) {
                Service.Logger.Error(exception, "Failed to schedule NativeMeters login startup.");
            }
        }

        return Task.CompletedTask;
    }


    private void OnFrameworkUpdate(IFramework framework) {
        if (isDisposed) return;

        System.MeterService.ProcessPendingMessages();

        if (System.Config.General.PreviewEnabled) System.TestMeterService.Tick();
    }

    private void OnLogin() {
        if (isDisposed || hasHandledLogin) return;

        hasHandledLogin = true;

        try {
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
        catch (Exception exception) {
            hasHandledLogin = false;
            Service.Logger.Error(exception, "Failed to enable NativeMeters services after login.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposed) return;
        isDisposed = true;

        try
        {
            Service.Framework.Update -= OnFrameworkUpdate;
            Service.ClientState.Login -= OnLogin;

            if (System.AddonConfigurationWindow is not null)
            {
                Service.PluginInterface.UiBuilder.OpenMainUi -= System.AddonConfigurationWindow.Toggle;
                Service.PluginInterface.UiBuilder.OpenConfigUi -= System.AddonConfigurationWindow.Toggle;
            }

            if (System.Config is not null)
            {
                ConfigRepository.SaveImmediately(System.Config);
            }

            System.CommandHandler?.Dispose();
            if (System.OverlayManager is not null)
            {
                await System.OverlayManager.DisposeAsync();
            }

            if (!Service.Framework.IsFrameworkUnloading)
            {
                await ColorInputRow.DisposeSharedColorPicker();
                if (System.TagSearchAddon is not null) await System.TagSearchAddon.DisposeAsync();
                if (System.IconSearchAddon is not null) await System.IconSearchAddon.DisposeAsync();
                if (System.AddonConfigurationWindow is not null) await System.AddonConfigurationWindow.DisposeAsync();
                if (System.AddonDetailedBreakdownWindow is not null) await System.AddonDetailedBreakdownWindow.DisposeAsync();
            }

            await Service.Framework.RunSafely(() => System.OverlayController?.Dispose());

            System.DtrService?.Dispose();
            System.TestMeterService?.Dispose();
            System.InternalMeterService?.Dispose();
            System.MeterService?.Dispose();

            await Service.Framework.RunSafely(KamiToolKitLibrary.Dispose);
        }
        finally
        {
            ConfigRepository.Clear();
            System.Clear();
            Service.Clear();
            PluginInterface = null!;
        }
    }
}
