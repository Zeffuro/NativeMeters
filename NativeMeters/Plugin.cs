using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;
using KamiToolKit.BaseTypes;
using NativeMeters.Addons;
using NativeMeters.Clients;
using NativeMeters.Commands;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Localization;
using NativeMeters.Models;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Configuration.Meter.Search;
using NativeMeters.Nodes.Configuration.Meter.Tags;
using NativeMeters.Nodes.Configuration.Setup;
using NativeMeters.Services;
using NativeMeters.Services.Internal;

namespace NativeMeters;

public class Plugin : IAsyncDalamudPlugin
{
    private bool hasHandledLogin;
    private bool hasOfferedSetup;
    private bool isDisposed;

    [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PluginInterface.Create<Service>();
        Loc.Initialize(Service.ClientState.ClientLanguage.ToString());
        System.Config = ConfigRepository.LoadOrDefault();
        Loc.SetLanguage(System.Config.General.UiLanguage);
        ConfigRepository.Save(System.Config);
        ConfigBackup.DoConfigBackup(Service.PluginInterface);

        await KamiToolKitLibrary.InitializeAsync(Service.PluginInterface);

        System.MeterService = new MeterService(new WebSocketClient(), new IINACTIpcClient());
        System.InternalMeterService = new InternalMeterService();
        System.TestMeterService = new TestMeterService();
        System.ActiveMeterService = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal
            ? System.InternalMeterService
            : System.MeterService;

        System.DtrService = new DtrService();

        System.TagSearchAddon = new TagSearchAddon
        {
            InternalName = "NativeMeters_TagPicker",
            Title = Loc.GetWindowHeader("WindowTagPicker").Title,
            Size = new Vector2(480, 600),
        };
        System.IconSearchAddon = new IconSearchAddon
        {
            InternalName = "NativeMeters_IconPicker",
            Title = Loc.GetWindowHeader("WindowIconPicker").Title,
        };
        System.TextFlagsPickerAddon = new TextFlagsPickerAddon
        {
            Title = Loc.GetWindowHeader("WindowTextFlags").Title,
            InternalName = "NativeMeters_TextFlags",
            Size = new Vector2(240.0f, 340.0f),
            RememberClosePosition = false,
        };

        System.AddonConfigurationWindow = new AddonConfigurationWindow
        {
            InternalName = "NativeMeters_Config",
            Title = Loc.GetWindowHeader("WindowConfiguration").Title,
            Size = new Vector2(800, 600),
        };

        System.AddonDetailedBreakdownWindow = new AddonDetailedBreakdownWindow
        {
            InternalName = "NativeMeters_Breakdown",
            Title = Loc.GetWindowHeader("WindowBreakdown").Title,
            Size = new Vector2(720, 560),
        };
        System.SetupWizard = new SetupWizardAddon
        {
            InternalName = "NativeMeters_Setup",
            Title = Loc.GetWindowHeader("WindowSetup").Title,
            Size = new Vector2(SetupControls.DialogWidth, SetupControls.DialogHeight),
            RememberClosePosition = false,
        };
        System.MeterPlacementAddon = new MeterPlacementAddon
        {
            InternalName = "NativeMeters_Place",
            Title = Loc.GetWindowHeader("WindowPlacement").Title,
            RememberClosePosition = false,
            EnableContextMenu = false,
        };

        Service.PluginInterface.UiBuilder.OpenMainUi += System.AddonConfigurationWindow.Toggle;
        Service.PluginInterface.UiBuilder.OpenConfigUi += System.AddonConfigurationWindow.Toggle;

        System.CommandHandler = new CommandHandler();

        System.PartyListMeterManager = new PartyListMeterManager();

        System.OverlayManager = new OverlayManager();
        System.OverlayManager.Setup();

        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;

        if (Service.ClientState.IsLoggedIn) {
            try {
                await Service.Framework.RunOnFrameworkThread(OnLogin);
            }
            catch (Exception exception) {
                Service.Logger.Error(exception, "Failed to schedule NativeMeters login startup.");
            }
        }
    }


    private void OnFrameworkUpdate(IFramework framework) {
        if (isDisposed) return;

        System.MeterService.ProcessPendingMessages();

        if (System.Config.General.PreviewEnabled) System.TestMeterService.Tick();
        System.OverlayManager.Tick();
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
            System.PartyListMeterManager.UpdateSettings();
            if (!hasOfferedSetup && System.Config.SetupStatus == SetupStatus.Pending)
            {
                System.SetupWizard.OpenForSetup();
                hasOfferedSetup = true;
            }
            else
            {
                System.AddonConfigurationWindow.DebugOpen();
            }
        }
        catch (Exception exception) {
            hasHandledLogin = false;
            Service.Logger.Error(exception, "Failed to enable NativeMeters services after login.");
        }
    }

    private void OnLogout(int type, int code)
    {
        if (isDisposed) return;

        hasHandledLogin = false;

        try
        {
            System.OverlayManager?.CancelPlacement();
            System.SetupWizard.Close();
            System.MeterService?.ResetLocalData();
            System.InternalMeterService?.ResetLocalData();
            System.TestMeterService?.ResetLocalData();
            System.PartyListMeterManager?.UpdateSettings();
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to reset NativeMeters state after logout.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposed) return;
        isDisposed = true;
        System.AddonConfigurationWindow?.CancelLanguageReopen();

        try
        {
            Service.Framework.Update -= OnFrameworkUpdate;
            Service.ClientState.Login -= OnLogin;
            Service.ClientState.Logout -= OnLogout;

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
            await Service.Framework.RunOnFrameworkThreadIfNeeded(() => System.PartyListMeterManager?.Dispose());

            if (System.OverlayManager is not null)
            {
                await System.OverlayManager.DisposeAsync();
            }
            await Service.Framework.RunOnFrameworkThreadIfNeeded(NodeDisposalExtensions.FlushPendingNodeDisposals);

            if (!Service.Framework.IsFrameworkUnloading)
            {
                // KTK waits for native window closure only on its asynchronous disposal path.
                await Task.Run(DisposeWindowsAsync);
            }

            await Service.Framework.RunOnFrameworkThreadIfNeeded(() => System.OverlayController?.Dispose());

            System.DtrService?.Dispose();
            System.TestMeterService?.Dispose();
            System.InternalMeterService?.Dispose();
            System.MeterService?.Dispose();

            await Service.Framework.RunOnFrameworkThreadIfNeeded(KamiToolKitLibrary.Dispose);
        }
        finally
        {
            ConfigRepository.Clear();
            System.Clear();
            Service.Clear();
            PluginInterface = null!;
        }
    }

    private static async Task DisposeWindowsAsync()
    {
        await ColorInputRow.DisposeSharedColorPicker();
        await DisposeNativeAddon(System.TagSearchAddon);
        await DisposeNativeAddon(System.IconSearchAddon);
        await DisposeNativeAddon(System.TextFlagsPickerAddon);
        await DisposeNativeAddon(System.SetupWizard);
        await DisposeNativeAddon(System.MeterPlacementAddon);
        await DisposeNativeAddon(System.AddonConfigurationWindow);
        await DisposeNativeAddon(System.AddonDetailedBreakdownWindow);
    }

    private static ValueTask DisposeNativeAddon(NativeAddon? addon)
        => addon?.DisposeAsync() ?? ValueTask.CompletedTask;
}
