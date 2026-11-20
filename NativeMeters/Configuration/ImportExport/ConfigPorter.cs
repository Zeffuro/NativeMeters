using Dalamud.Bindings.ImGui;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Services;
using NativeMeters.Localization;

namespace NativeMeters.Configuration.ImportExport;

public abstract class ConfigPorter {
    public static void TryImportConfigFromClipboard()
    {
        var clipboard = ClipboardService.GetClipboardText();

        if (!string.IsNullOrWhiteSpace(clipboard))
        {
            var imported = ConfigSerializer.DeserializeConfig(clipboard);
            if (imported != null)
            {
                imported.EnsureInitialized();
                if (imported.SetupStatus == SetupStatus.Pending)
                    imported.SetupStatus = SetupStatus.Finished;
                System.Config = imported;
                Loc.SetLanguage(imported.General.UiLanguage);
                ConnectionSelection.ApplyCurrent();
                ConfigRepository.Save(System.Config);
                Service.NotificationManager.Success(Loc.Get("NotifyConfigImported"));
            }
            else
            {
                Service.NotificationManager.Error(Loc.Get("NotifyConfigImportFailed"));
            }
        }
        else
        {
            Service.NotificationManager.Warning(Loc.Get("NotifyConfigClipboardInvalid"));
        }
    }

    public static void TryExportConfigToClipboard(
        SystemConfiguration config)
    {
        var exportString = ConfigSerializer.SerializeConfig(config);
        ClipboardService.SetClipboardText(exportString);
        Service.NotificationManager.Success(Loc.Get("NotifyConfigExported"));
    }

    public static void TryResetConfig()
    {
        System.Config = ConfigRepository.Reset();
        Loc.SetLanguage(System.Config.General.UiLanguage);
        ConnectionSelection.ApplyCurrent();
        ConfigRepository.Save(System.Config);
        Service.NotificationManager.Success(Loc.Get("NotifyConfigReset"));
    }

    public static void TryExportMeterToClipboard(MeterSettings meter)
    {
        var exportString = ConfigSerializer.SerializeCompressed(meter);

        ClipboardService.SetClipboardText(exportString);
        Service.NotificationManager.Success(Loc.Format("NotifyMeterExported", meter.Name));
    }

    public static MeterSettings? TryImportMeterFromClipboard()
    {
        var imported = ReadMeterFromClipboard();
        if (imported != null) Service.NotificationManager.Success(Loc.Get("NotifyMeterImported"));
        return imported;
    }

    internal static MeterSettings? ReadMeterFromClipboard()
    {
        var clipboard = ClipboardService.GetClipboardText();

        if (string.IsNullOrWhiteSpace(clipboard))
        {
            Service.NotificationManager.Warning(Loc.Get("NotifyClipboardEmpty"));
            return null;
        }

        try
        {
            var imported = ConfigSerializer.DeserializeCompressed<MeterSettings>(clipboard);

            if (imported == null)
            {
                Service.NotificationManager.Error(Loc.Get("NotifyClipboardParseFailed"));
                return null;
            }

            imported.EnsureInitialized();
            return imported;
        }
        catch
        {
            Service.NotificationManager.Error(Loc.Get("NotifyClipboardMeterInvalid"));
            return null;
        }
    }
}
