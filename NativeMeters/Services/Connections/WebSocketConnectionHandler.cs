using System;
using NativeMeters.Clients;

namespace NativeMeters.Services.Connections;

public class WebSocketConnectionHandler(WebSocketClient client) : IConnectionHandler
{
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnMessageReceived;

    private Action? connectedHandler;
    private Action? disconnectedHandler;
    private Action<string>? messageHandler;

    public bool IsConnected => client.IsConnected;

    public void Start()
    {
        connectedHandler = () => OnConnected?.Invoke();
        disconnectedHandler = () => OnDisconnected?.Invoke();
        messageHandler = msg => OnMessageReceived?.Invoke(msg);

        client.OnConnected += connectedHandler;
        client.OnDisconnected += disconnectedHandler;
        client.OnMessageReceived += messageHandler;

        _ = client.StartAsync(new Uri(System.Config.ConnectionSettings.WebSocketUrl));
    }

    public void Stop()
    {
        if (connectedHandler != null) client.OnConnected -= connectedHandler;
        if (disconnectedHandler != null) client.OnDisconnected -= disconnectedHandler;
        if (messageHandler != null) client.OnMessageReceived -= messageHandler;

        connectedHandler = null;
        disconnectedHandler = null;
        messageHandler = null;

        _ = client.StopAsync();
    }

    public void Dispose() => Stop();
}
