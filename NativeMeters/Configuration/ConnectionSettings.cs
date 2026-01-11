using NativeMeters.Models;

namespace NativeMeters.Configuration;

public class ConnectionSettings
{

    public ConnectionType SelectedConnectionType { get; set; } = ConnectionType.WebSocket;
    public string WebSocketUrl { get; set; } = "ws://127.0.0.1:10501/ws";

    public bool AutoReconnect { get; set; } = true;
    public int AutoReconnectInterval { get; set; } = 30;

    public bool LogConnectionErrors { get; set; } = false;
}