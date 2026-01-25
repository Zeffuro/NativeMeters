using System;

namespace NativeMeters.Services;

public class ReconnectionManager
{
    private DateTime lastAttempt = DateTime.MinValue;
    private bool reconnectPending;

    public bool ShouldReconnect(bool isConnected, bool isManuallyDisabled)
    {
        if (isManuallyDisabled || isConnected) return false;
        if (!System.Config.ConnectionSettings.AutoReconnect) return false;

        var interval = System.Config.ConnectionSettings.AutoReconnectInterval;
        if ((DateTime.Now - lastAttempt).TotalSeconds >= interval)
        {
            lastAttempt = DateTime.Now;
            return true;
        }
        return false;
    }

    public void RequestReconnect() => reconnectPending = true;

    public bool ConsumePendingReconnect()
    {
        if (!reconnectPending) return false;
        reconnectPending = false;
        return true;
    }

    public void MarkDisconnected() => lastAttempt = DateTime.Now;
}
