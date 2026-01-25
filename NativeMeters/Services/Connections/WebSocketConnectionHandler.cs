using System;
using NativeMeters.Clients;

namespace NativeMeters.Services.Connections;

public class WebSocketConnectionHandler(WebSocketClient client) : IConnectionHandler
{
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnMessageReceived;

    public bool IsConnected => client.IsConnected;

    public void Start()
    {
        client.OnConnected += () => OnConnected?.Invoke();
        client.OnDisconnected += () => OnDisconnected?.Invoke();
        client.OnMessageReceived += msg => OnMessageReceived?.Invoke(msg);
        _ = client.StartAsync(new Uri(System.Config.ConnectionSettings.WebSocketUrl));
    }

    public void Stop()
    {
        client.OnConnected -= () => OnConnected?.Invoke();
        client.OnDisconnected -= () => OnDisconnected?.Invoke();
        client.OnMessageReceived -= msg => OnMessageReceived?.Invoke(msg);
        _ = client.StopAsync();
    }

    public void Dispose() => client.Dispose();
}
