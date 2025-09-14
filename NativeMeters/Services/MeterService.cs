using System;
using System.Collections.Concurrent;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using NativeMeters.Config;

namespace NativeMeters.Services;

public class MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    : MeterServiceBase, IDisposable
{
    private readonly ConcurrentQueue<string> webSocketMessageQueue = new();
    private readonly ConcurrentQueue<string> ipcMessageQueue = new();
    private ConnectionType CurrentConnectionType => ConnectionConfig.Instance.SelectedConnectionType;
    private string ServerUri => ConnectionConfig.Instance.WebSocketUrl;

    public override event Action? CombatDataUpdated;

    public void Enable()
    {
        switch (CurrentConnectionType)
        {
            case ConnectionType.WebSocket:
                _ = webSocketClient.StartAsync(new Uri(ServerUri));
                webSocketClient.OnMessageReceived += EnqueueWebSocketMessage;
                break;
            case ConnectionType.IINACTIPC:
                iinactIpcClient.Subscribe();
                break;
            default:
                Service.Logger.Error($"Unknown connection type: {CurrentConnectionType}");
                break;
        }
    }

    private void EnqueueWebSocketMessage(string message)
    {
        webSocketMessageQueue.Enqueue(message);
    }

    public void EnqueueIpcMessage(string message)
    {
        ipcMessageQueue.Enqueue(message);
    }

    public void ProcessPendingMessages()
    {
        switch (CurrentConnectionType)
        {
            case ConnectionType.WebSocket:
                while (webSocketMessageQueue.TryDequeue(out var message))
                {
                    HandleMessage(message);
                }
                break;
            case ConnectionType.IINACTIPC:
                while (ipcMessageQueue.TryDequeue(out var ipcMessage))
                {
                    HandleMessage(ipcMessage);
                }
                break;
            default:
                Service.Logger.Error($"Unknown connection type: {CurrentConnectionType}");
                break;
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            var combatDataMessage = JsonSerializer.Deserialize<CombatDataMessage>(message);
            if (combatDataMessage == null) return;

            CombatData = combatDataMessage;
            CombatDataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            Service.Logger.Error($"CombatDataMessage deserialization error: {ex}");
        }
    }

    public override bool IsConnected => CurrentConnectionType == ConnectionType.WebSocket
        ? webSocketClient.IsConnected
        : iinactIpcClient.IsConnected;

    public void Dispose()
    {
        switch (CurrentConnectionType)
        {
            case ConnectionType.WebSocket:
                webSocketClient.OnMessageReceived -= EnqueueWebSocketMessage;
                webSocketClient.Dispose();
                break;
            case ConnectionType.IINACTIPC:
                iinactIpcClient.Unsubscribe();
                break;
            default:
                Service.Logger.Error($"Unknown connection type: {CurrentConnectionType}");
                break;
        }

        webSocketMessageQueue.Clear();
        ipcMessageQueue.Clear();
        CombatData = null;
    }
}