using System;
using System.Collections.Concurrent;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Interface.ImGuiNotification;
using NativeMeters.Configuration;
using NativeMeters.Extensions;

namespace NativeMeters.Services;

public class MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    : MeterServiceBase, IDisposable
{
    private DateTime _lastReconnectAttempt = DateTime.MinValue;
    private bool _isManuallyDisabled;
    private bool _reconnectPending;

    private readonly ConcurrentQueue<string> webSocketMessageQueue = new();
    private readonly ConcurrentQueue<string> ipcMessageQueue = new();
    private ConnectionType CurrentConnectionType => System.Config.ConnectionSettings.SelectedConnectionType;
    private string ServerUri => System.Config.ConnectionSettings.WebSocketUrl;

    public void Enable()
    {
        switch (CurrentConnectionType)
        {
            case ConnectionType.WebSocket:
                webSocketClient.OnConnected += ShowConnectionNotification;
                webSocketClient.OnDisconnected += HandleDisconnect;
                _ = webSocketClient.StartAsync(new Uri(ServerUri));
                webSocketClient.OnMessageReceived += EnqueueWebSocketMessage;
                break;
            case ConnectionType.IINACTIPC:
                iinactIpcClient.OnConnected += ShowConnectionNotification;
                iinactIpcClient.Subscribe();
                break;
            default:
                Service.Logger.Error($"Unknown connection type: {CurrentConnectionType}");
                break;
        }
    }

    private void ShowConnectionNotification()
    {
        string service = CurrentConnectionType == ConnectionType.WebSocket ? "ACT" : "IINACT";
        Service.NotificationManager.AddNotification(new Notification
        {
            Content = $"[NativeMeters] Successfully connected to {service}.\nNativeMeters can now display Meters.",
            Type = NotificationType.Success,
        });
    }

    private void CheckAutoReconnect()
    {
        if (_isManuallyDisabled) return;
        if (IsConnected) return;
        if (!System.Config.ConnectionSettings.AutoReconnect) return;

        var interval = System.Config.ConnectionSettings.AutoReconnectInterval;
        if ((DateTime.Now - _lastReconnectAttempt).TotalSeconds >= interval)
        {
            _lastReconnectAttempt = DateTime.Now;
            Service.Logger.DebugOnly("Attempting auto-reconnect...");
            ActualReconnect();
        }
    }

    public void Reconnect() => RequestReconnect();

    public void RequestReconnect()
    {
        _reconnectPending = true;
    }

    public void ActualReconnect()
    {
        StopClients();
        Enable();
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
        try
        {
            if (_reconnectPending)
            {
                _reconnectPending = false;
                ActualReconnect();
                return;
            }

            CheckAutoReconnect();

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
        catch (Exception ex)
        {
            Service.Logger.Error($"Error in ProcessPendingMessages: {ex.Message}");
        }
    }

    private void HandleDisconnect()
    {
        _lastReconnectAttempt = DateTime.Now;
    }

    private void HandleMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        try
        {
            var jsonNode = JsonNode.Parse(message);
            if (jsonNode == null) return;

            var messageType = jsonNode["type"]?.ToString() ?? jsonNode["Type"]?.ToString();

            if (messageType is "CombatData" or "broadcast")
            {
                Service.Logger.DebugOnly($"Received combat message: {message}");
                var combatDataMessage = JsonSerializer.Deserialize<CombatDataMessage>(message);
                if (combatDataMessage == null) return;

                CombatData = combatDataMessage;

                InvokeCombatDataUpdated();
            }
            else
            {
                Service.Logger.DebugOnly($"Received non-combat message type: {messageType}");
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

    private void StopClients()
    {
        webSocketClient.OnConnected -= ShowConnectionNotification;
        webSocketClient.OnDisconnected -= HandleDisconnect;
        webSocketClient.OnMessageReceived -= EnqueueWebSocketMessage;

        _ = webSocketClient.StopAsync();

        iinactIpcClient.OnConnected -= ShowConnectionNotification;
        iinactIpcClient.Unsubscribe();
    }

    public void Dispose()
    {
        _isManuallyDisabled = true;
        StopClients();
        webSocketClient.Dispose();

        webSocketMessageQueue.Clear();
        ipcMessageQueue.Clear();
        CombatData = null;
    }
}
