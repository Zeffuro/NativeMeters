using System;
using NativeMeters.Clients;

namespace NativeMeters.Services.Connections;

public class IINACTConnectionHandler(IINACTIpcClient client) : IConnectionHandler
{
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnMessageReceived;

    public bool IsConnected => client.IsConnected;

    public void Start()
    {
        client.OnConnected += HandleConnected;
        client.OnDisconnected += HandleDisconnected;
        client.OnMessageReceived += HandleMessageReceived;
        client.Subscribe();
    }

    private void HandleConnected() => OnConnected?.Invoke();
    private void HandleDisconnected() => OnDisconnected?.Invoke();
    private void HandleMessageReceived(string message) => OnMessageReceived?.Invoke(message);

    public void Stop()
    {
        client.OnConnected -= HandleConnected;
        client.OnDisconnected -= HandleDisconnected;
        client.OnMessageReceived -= HandleMessageReceived;
        client.Unsubscribe();
    }

    public void Dispose() => Stop();
}
