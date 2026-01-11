using System;
using System.Collections.Concurrent;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Services;

public class MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    : MeterServiceBase, IDisposable
{
    private readonly ConcurrentQueue<string> webSocketMessageQueue = new();
    private readonly ConcurrentQueue<string> ipcMessageQueue = new();
    private ConnectionType CurrentConnectionType => System.Config.ConnectionSettings.SelectedConnectionType;
    private string ServerUri => System.Config.ConnectionSettings.WebSocketUrl;

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
            var jsonNode = JsonNode.Parse(message);
            if (jsonNode == null) return;

            var messageType = jsonNode["type"]?.ToString() ?? jsonNode["Type"]?.ToString();

            if (messageType is "CombatData" or "broadcast")
            {
                Service.Logger.Debug($"Received combat message: {message}");
                var combatDataMessage = JsonSerializer.Deserialize<CombatDataMessage>(message);
                if (combatDataMessage == null) return;

                CombatData = combatDataMessage;
                CombatDataUpdated?.Invoke();
            }
            else
            {
                // Log or ignore other message types like 'connection' or 'subscribe'
                Service.Logger.Debug($"Received non-combat message type: {messageType}");
            }
        }
        catch (JsonException ex)
        {
            Service.Logger.Error($"JSON structure error: {ex.Message}");
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