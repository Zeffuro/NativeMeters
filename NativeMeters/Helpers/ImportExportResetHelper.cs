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
}
