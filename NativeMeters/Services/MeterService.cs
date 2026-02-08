using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using NativeMeters.Clients;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Services.Connections;

namespace NativeMeters.Services;

public class MeterService : MeterServiceBase, IDisposable
{
    private readonly WebSocketClient webSocketClient;
    private readonly IINACTIpcClient iinactIpcClient;
    private readonly ReconnectionManager reconnectionManager = new();
    private readonly CombatStateTracker combatStateTracker = new();
    private readonly ConcurrentQueue<string> messageQueue = new();

    private IConnectionHandler? activeConnection;
    private bool isManuallyDisabled;

    public MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    {
        this.webSocketClient = webSocketClient;
        this.iinactIpcClient = iinactIpcClient;
    }

    public void Enable()
    {
        if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
            return;

        activeConnection = CreateConnectionHandler();
        activeConnection.OnConnected += ShowConnectionNotification;
        activeConnection.OnDisconnected += reconnectionManager.MarkDisconnected;
        activeConnection.OnMessageReceived += msg => messageQueue.Enqueue(msg);
        activeConnection.Start();
    }

    private IConnectionHandler CreateConnectionHandler()
    {
        return System.Config.ConnectionSettings.SelectedConnectionType switch
        {
            ConnectionType.WebSocket => new WebSocketConnectionHandler(webSocketClient),
            ConnectionType.IINACTIPC => new IINACTConnectionHandler(iinactIpcClient),
            _ => throw new InvalidOperationException("Unknown connection type")
        };
    }

    public void EnqueueIpcMessage(string message) => messageQueue.Enqueue(message);

    public void ProcessPendingMessages()
    {
        if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
            return;

        if (reconnectionManager.ConsumePendingReconnect())
        {
            Reconnect();
            return;
        }

        if (reconnectionManager.ShouldReconnect(IsConnected, isManuallyDisabled))
            Reconnect();

        if (combatStateTracker.CheckCombatEnded())
            EndEncounter();

        while (messageQueue.TryDequeue(out var message))
            HandleMessage(message);
    }

    private void HandleMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        try
        {
            var jsonNode = JsonNode.Parse(message);
            var messageType = jsonNode?["type"]?.ToString() ?? jsonNode?["Type"]?.ToString();

            if (messageType is "CombatData" or "broadcast")
            {
                var newData = JsonSerializer.Deserialize<CombatDataMessage>(message, JsonSerializerConfig.CaseSensitive);
                HandleNewCombatData(newData);
                InvokeCombatDataUpdated();
            }
        }
        catch (Exception ex)
        {
            Service.Logger.Error($"Message handling error: {ex.Message}");
        }
    }

    public void Reconnect()
    {
        activeConnection?.Stop();
        activeConnection?.Dispose();
        activeConnection = null;
        Enable();
    }

    public void RequestReconnect() => reconnectionManager.RequestReconnect();
    public void ClearMeter() { CombatData = null; InvokeCombatDataUpdated(); SendChatCommand("clear"); }
    public void EndEncounter()
    {
        if (System.Config.General.EnableEncounterHistory)
        {
            ArchiveCurrentEncounter();
        }
        SendChatCommand("end");
    }

    private void SendChatCommand(string command)
    {
        if (command == "clear" && !System.Config.General.ClearActWithMeter) return;
        Service.ChatGui.Print(new XivChatEntry { Message = command, Type = XivChatType.Echo });
    }

    private void ShowConnectionNotification()
    {
        var service = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.WebSocket ? "ACT" : "IINACT";
        Service.NotificationManager.Success($"Connected to {service}.");
    }

    public override bool IsConnected => activeConnection?.IsConnected ?? false;

    public void Dispose()
    {
        isManuallyDisabled = true;
        activeConnection?.Dispose();
        messageQueue.Clear();
        CombatData = null;
    }
}
