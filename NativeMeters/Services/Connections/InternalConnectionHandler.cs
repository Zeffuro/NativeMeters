using System;

namespace NativeMeters.Services.Connections;

public class InternalConnectionHandler : IConnectionHandler
{
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<string>? OnMessageReceived;

    private InternalCombatProcessor? processor;
    private bool isConnected;

    public bool IsConnected => isConnected;

    public void Start()
    {
        processor = new InternalCombatProcessor();
        processor.OnCombatDataJson += json => OnMessageReceived?.Invoke(json);
        processor.Enable();

        isConnected = true;
        OnConnected?.Invoke();
    }

    public void Stop()
    {
        isConnected = false;
        processor?.Dispose();
        processor = null;
        OnDisconnected?.Invoke();
    }

    public void Dispose() => Stop();
}
