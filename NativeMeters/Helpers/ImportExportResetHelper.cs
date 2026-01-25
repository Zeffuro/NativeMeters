using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using NativeMeters.Configuration;
using NativeMeters.Services;

namespace NativeMeters.Helpers;

public abstract class ImportExportResetHelper {
    public static void TryImportConfigFromClipboard()
    {
        var clipboard = ImGui.GetClipboardText();
        var notification = new Notification { Content = "Configuration imported from clipboard.", Type = NotificationType.Success };

        if (!string.IsNullOrWhiteSpace(clipboard))
        {
            var imported = Util.DeserializeConfig(clipboard);
            if (imported != null)
            {
                System.Config = imported;
                Util.SaveConfig(System.Config);
                Service.Logger.Info("Configuration imported from clipboard.");
            }
            else
            {
                notification.Content = "Clipboard data was invalid or could not be imported.";
                notification.Type = NotificationType.Error;
                Service.Logger.Warning("Clipboard data was invalid or could not be imported.");
            }
        }
        else
        {
            notification.Content = "Clipboard is empty or invalid for import.";
            notification.Type = NotificationType.Warning;
            Service.Logger.Warning("Clipboard is empty or invalid for import.");
        }

        Service.NotificationManager.AddNotification(notification);
    }

    public static void TryExportConfigToClipboard(
        SystemConfiguration config)
    {
        var exportString = Util.SerializeConfig(config);
        ImGui.SetClipboardText(exportString);
        Service.NotificationManager.AddNotification(
            new Notification { Content = "Configuration exported to clipboard.", Type = NotificationType.Success }
        );
        Service.Logger.Info("Configuration exported to clipboard.");
    }

    public static void TryResetConfig()
    {
        System.Config = Util.ResetConfig();
        Util.SaveConfig(System.Config);

        Service.NotificationManager.AddNotification(
            new Notification { Content = "Configuration reset to default.", Type = NotificationType.Success }
        );
        Service.Logger.Info("Configuration reset to default.");
    }

    public static void TryExportMeterToClipboard(MeterSettings meter)
    {
        var exportString = Util.SerializeCompressed(meter);

        ImGui.SetClipboardText(exportString);
        Service.NotificationManager.AddNotification(
            new Notification { Content = $"Meter '{meter.Name}' exported to clipboard.", Type = NotificationType.Success }
        );
        Service.Logger.Info($"Meter '{meter.Name}' exported to clipboard.");
    }

    public static MeterSettings? TryImportMeterFromClipboard()
    {
        var clipboard = ImGui.GetClipboardText();

        if (string.IsNullOrWhiteSpace(clipboard))
        {
            Service.NotificationManager.AddNotification(
                new Notification { Content = "Clipboard is empty.", Type = NotificationType.Warning }
            );
            return null;
        }

        try
        {
            var imported = Util.DeserializeCompressed<MeterSettings>(clipboard);

            if (imported == null)
            {
                Service.NotificationManager.AddNotification(
                    new Notification { Content = "Clipboard data could not be parsed.", Type = NotificationType.Error }
                );
                return null;
            }

            Service.NotificationManager.AddNotification(
                new Notification { Content = "Meter settings imported.", Type = NotificationType.Success }
            );
            return imported;
        }
        catch
        {
            Service.NotificationManager.AddNotification(
                new Notification { Content = "Invalid meter data in clipboard.", Type = NotificationType.Error }
            );
            return null;
        }
    }
}
