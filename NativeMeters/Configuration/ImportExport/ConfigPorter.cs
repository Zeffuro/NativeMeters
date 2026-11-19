using Dalamud.Bindings.ImGui;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Services;

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
                ConnectionSelection.ApplyCurrent();
                ConfigRepository.Save(System.Config);
                Service.NotificationManager.Success("Configuration imported from clipboard.");
            }
            else
            {
                Service.NotificationManager.Error("Clipboard data was invalid or could not be imported.");
            }
        }
        else
        {
            Service.NotificationManager.Warning("Clipboard is empty or invalid for import.");
        }
    }

    public static void TryExportConfigToClipboard(
        SystemConfiguration config)
    {
        var exportString = ConfigSerializer.SerializeConfig(config);
        ClipboardService.SetClipboardText(exportString);
        Service.NotificationManager.Success("Configuration exported to clipboard.");
    }

    public static void TryResetConfig()
    {
        System.Config = ConfigRepository.Reset();
        ConnectionSelection.ApplyCurrent();
        ConfigRepository.Save(System.Config);
        Service.NotificationManager.Success("Configuration reset to default.");
    }

    public static void TryExportMeterToClipboard(MeterSettings meter)
    {
        var exportString = ConfigSerializer.SerializeCompressed(meter);

        ClipboardService.SetClipboardText(exportString);
        Service.NotificationManager.Success($"Meter '{meter.Name}' exported to clipboard.");
    }

    public static MeterSettings? TryImportMeterFromClipboard()
    {
        var imported = ReadMeterFromClipboard();
        if (imported != null) Service.NotificationManager.Success("Meter settings imported.");
        return imported;
    }

    internal static MeterSettings? ReadMeterFromClipboard()
    {
        var clipboard = ClipboardService.GetClipboardText();

        if (string.IsNullOrWhiteSpace(clipboard))
        {
            Service.NotificationManager.Warning("Clipboard is empty.");
            return null;
        }

        try
        {
            var imported = ConfigSerializer.DeserializeCompressed<MeterSettings>(clipboard);

            if (imported == null)
            {
                Service.NotificationManager.Error("Clipboard data could not be parsed.");
                return null;
            }

            imported.EnsureInitialized();
            return imported;
        }
        catch
        {
            Service.NotificationManager.Error("Invalid meter data in clipboard.");
            return null;
        }
    }
}
