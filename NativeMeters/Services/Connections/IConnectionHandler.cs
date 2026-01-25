using System;

namespace NativeMeters.Services.Connections;

public interface IConnectionHandler : IDisposable
{
    event Action? OnConnected;
    event Action? OnDisconnected;
    event Action<string>? OnMessageReceived;
    bool IsConnected { get; }
    void Start();
    void Stop();
}
