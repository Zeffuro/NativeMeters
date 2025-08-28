using System;
using System.Collections.Concurrent;
using NativeMeters.Clients;
using NativeMeters.Models;
using System.Text.Json;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace NativeMeters.Services;

public class MeterService : IDisposable, IMeterService
{
    private readonly WebSocketClient _webSocketClient;
    private readonly IINACTIpcClient iinactIpcClient;
    private readonly ConcurrentQueue<string> _webSocketMessageQueue = new();
    private readonly ConcurrentQueue<string> _ipcMessageQueue = new();
    private CombatDataMessage? _currentCombatData;
    private bool _useWebSocket = true;

    public event Action? CombatDataUpdated;

    public MeterService(WebSocketClient webSocketClient, IINACTIpcClient iinactIpcClient)
    {
        _webSocketClient = webSocketClient;
        this.iinactIpcClient = iinactIpcClient;


    }

    public void Enable()
    {
        if (_useWebSocket)
        {
            var serverUri = new Uri("ws://127.0.0.1:10501/ws");
            _ = _webSocketClient.StartAsync(serverUri);
            _webSocketClient.OnMessageReceived += EnqueueWebSocketMessage;
        }
        else
        {
            iinactIpcClient.Subscribe();
        }
    }

    public CombatDataMessage? CurrentCombatData => _currentCombatData;

    private void EnqueueWebSocketMessage(string message)
    {
        _webSocketMessageQueue.Enqueue(message);
    }

    public void EnqueueIpcMessage(string message)
    {
        _ipcMessageQueue.Enqueue(message);
    }

    public void ProcessPendingMessages()
    {
        if (_useWebSocket)
        {
            while (_webSocketMessageQueue.TryDequeue(out var message))
            {
                HandleMessage(message);
            }
        }
        else
        {
            while (_ipcMessageQueue.TryDequeue(out var ipcMessage))
            {
                HandleMessage(ipcMessage);
            }
        }
    }

    private void HandleMessage(string message)
    {
        try
        {
            var combatData = JsonSerializer.Deserialize<CombatDataMessage>(message);
            if (combatData != null)
            {
                _currentCombatData = combatData;
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
        if (_useWebSocket)
        {
            _webSocketClient.OnMessageReceived -= EnqueueWebSocketMessage;
            _webSocketClient.Dispose();
        }
        else
        {
            iinactIpcClient.Unsubscribe();
        }

        _webSocketMessageQueue.Clear();
        _ipcMessageQueue.Clear();
        _currentCombatData = null;
    }
}