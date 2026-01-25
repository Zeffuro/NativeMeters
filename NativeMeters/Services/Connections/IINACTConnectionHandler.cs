using System;
using NativeMeters.Clients;

namespace NativeMeters.Services.Connections;

public class IINACTConnectionHandler(IINACTIpcClient client, Action<string> enqueueMessage) : IConnectionHandler
{
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnMessageReceived;

    public bool IsConnected => client.IsConnected;

    public void Start()
    {
        client.OnConnected += HandleConnected;
        client.Subscribe();
    }

    private void HandleConnected() => OnConnected?.Invoke();

    public void EnqueueMessage(string message) => OnMessageReceived?.Invoke(message);

    public void Stop()
    {
        client.OnConnected -= HandleConnected;
        client.Unsubscribe();
    }

    public void Dispose() => Stop();
}
