using System;
using System.Collections.Concurrent;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using NativeMeters.Configuration;
using NativeMeters.Extensions;

namespace NativeMeters.Services;

public class MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    : MeterServiceBase, IDisposable
{
    private DateTime lastReconnectAttempt = DateTime.MinValue;
    private bool isManuallyDisabled;
    private bool reconnectPending;
    private bool wasInCombat;

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

    public void ClearMeter()
    {
        CombatData = null;
        InvokeCombatDataUpdated();

        if (!System.Config.General.ClearActWithMeter) return;

        // I hate that we have to do this but ACT has no handlers for doing this through websocket.
        XivChatEntry clearMessage = new XivChatEntry
        {
            Message = "clear", Type = XivChatType.Echo
        };
        Service.ChatGui.Print(clearMessage);
    }

    public void EndEncounter()
    {
        // I hate that we have to do this but ACT has no handlers for doing this through websocket.
        XivChatEntry endMessage = new XivChatEntry
        {
            Message = "end", Type = XivChatType.Echo
        };
        Service.ChatGui.Print(endMessage);

    }

    private void CheckForceEndCombat()
    {
        if (!System.Config.General.ForceEndEncounter) return;

        bool isInCombat = Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat];

        if (wasInCombat && !isInCombat)
        {
            Service.Logger.Debug("Combat ended, forcing ACT encounter end.");
            EndEncounter();
        }

        wasInCombat = isInCombat;
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
        if (isManuallyDisabled) return;
        if (IsConnected) return;
        if (!System.Config.ConnectionSettings.AutoReconnect) return;

        var interval = System.Config.ConnectionSettings.AutoReconnectInterval;
        if ((DateTime.Now - lastReconnectAttempt).TotalSeconds >= interval)
        {
            lastReconnectAttempt = DateTime.Now;
            Service.Logger.DebugOnly("Attempting auto-reconnect...");
            ActualReconnect();
        }
    }

    public void Reconnect() => RequestReconnect();

    public void RequestReconnect()
    {
        reconnectPending = true;
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
            if (reconnectPending)
            {
                reconnectPending = false;
                ActualReconnect();
                return;
            }

            CheckAutoReconnect();
            CheckForceEndCombat();

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
        lastReconnectAttempt = DateTime.Now;
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
        isManuallyDisabled = true;
        StopClients();
        webSocketClient.Dispose();

        webSocketMessageQueue.Clear();
        ipcMessageQueue.Clear();
        CombatData = null;
    }
}
