using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace NativeMeters.Services;

public class MeterService : MeterServiceBase, IDisposable
{
    private readonly WebSocketClient webSocketClient;
    private readonly IINACTIpcClient iinactIpcClient;
    private readonly ConcurrentQueue<string> webSocketMessageQueue = new();
    private readonly ConcurrentQueue<string> ipcMessageQueue = new();
    private readonly bool useWebSocket = false;

    public override event Action? CombatDataUpdated;

    public MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    {
        this.webSocketClient = webSocketClient;
        this.iinactIpcClient = iinactIpcClient;
    }

    public void Enable()
    {
        if (useWebSocket)
        {
            var serverUri = new Uri("ws://127.0.0.1:10501/ws");
            _ = webSocketClient.StartAsync(serverUri);
            webSocketClient.OnMessageReceived += EnqueueWebSocketMessage;
        }
        else
        {
            iinactIpcClient.Subscribe();
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
        if (useWebSocket)
        {
            while (webSocketMessageQueue.TryDequeue(out var message))
            {
                HandleMessage(message);
            }
        }
        else
        {
            while (ipcMessageQueue.TryDequeue(out var ipcMessage))
            {
                HandleMessage(ipcMessage);
            }
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            var combatDataMessage = JsonSerializer.Deserialize<CombatDataMessage>(message);
            if (combatDataMessage != null)
            {
                CombatData = combatDataMessage;
                CombatDataUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Service.Logger.Error($"CombatDataMessage deserialization error: {ex}");
        }
    }

    public void Dispose()
    {
        if (useWebSocket)
        {
            webSocketClient.OnMessageReceived -= EnqueueWebSocketMessage;
            webSocketClient.Dispose();
        }
        else
        {
            iinactIpcClient.Unsubscribe();
        }

        webSocketMessageQueue.Clear();
        ipcMessageQueue.Clear();
        CombatData = null;
    }
}