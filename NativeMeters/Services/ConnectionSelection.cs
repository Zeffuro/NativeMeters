using NativeMeters.Models;
using NativeMeters.Services.Internal;

namespace NativeMeters.Services;

internal static class ConnectionSelection
{
    public static void Apply(ConnectionType type, string? websocketUrl = null)
    {
        var settings = System.Config.ConnectionSettings;
        var typeChanged = settings.SelectedConnectionType != type;
        var urlChanged = websocketUrl is not null && settings.WebSocketUrl != websocketUrl;
        if (!typeChanged && !urlChanged) return;

        settings.SelectedConnectionType = type;
        if (websocketUrl is not null) settings.WebSocketUrl = websocketUrl;
        ApplyCurrent();
    }

    public static void ApplyCurrent()
    {
        var type = System.Config.ConnectionSettings.SelectedConnectionType;
        if (type == ConnectionType.Internal || System.Config.General.EnableInternalParserForBreakdown)
        {
            if (System.InternalMeterService.IsDisposed)
            {
                System.InternalMeterService = new InternalMeterService();
                System.AddonDetailedBreakdownWindow.ReSubscribeToEvents();
            }

            System.InternalMeterService.Enable();
        }
        else
        {
            System.InternalMeterService.Dispose();
        }

        System.MeterService.Reconnect();
        System.OverlayManager.UpdateActiveService();
        System.PartyListMeterManager.UpdateSettings();
    }
}
