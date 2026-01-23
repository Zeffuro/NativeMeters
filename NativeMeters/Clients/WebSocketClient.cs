using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Clients;

public class WebSocketClient : IDisposable
{
    private ClientWebSocket? client;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly ConcurrentQueue<string> messageQueue = new();

    private readonly SemaphoreSlim connectionLock = new(1, 1);

    public event Action<string>? OnMessageReceived;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    private bool LogConnectionErrors => System.Config.ConnectionSettings.LogConnectionErrors;

    public async Task StartAsync(Uri serverUri)
    {
        await connectionLock.WaitAsync();
        try
        {
            await StopAsyncInternal();

            var newClient = new ClientWebSocket();
            var newCts = new CancellationTokenSource();

            client = newClient;
            cancellationTokenSource = newCts;

            await newClient.ConnectAsync(serverUri, newCts.Token);

            var subscription = new SubscriptionRequest();
            var subJson = JsonSerializer.Serialize(subscription);
            var buffer = Encoding.UTF8.GetBytes(subJson);

            await newClient.SendAsync(buffer, WebSocketMessageType.Text, true, newCts.Token);

            OnConnected?.Invoke();

            _ = Task.Run(() => ReceiveLoop(newClient, newCts.Token));
        }
        catch (Exception ex)
        {
            if (LogConnectionErrors) Service.Logger.Error($"WebSocket error: {ex.Message}");
            OnDisconnected?.Invoke();
        }
        finally
        {
            connectionLock.Release();
        }
    }

    private async Task ReceiveLoop(ClientWebSocket socket, CancellationToken token)
    {
        var buffer = new byte[4096];
        try
        {
            while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                using var ms = new global::System.IO.MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(buffer, token);
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                var message = Encoding.UTF8.GetString(ms.ToArray());
                if (!string.IsNullOrWhiteSpace(message))
                {
                    messageQueue.Enqueue(message);
                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        catch
        {
            if (socket == client) OnDisconnected?.Invoke();
        }
    }

    public bool IsConnected => client?.State == WebSocketState.Open;

    public async Task StopAsync()
    {
        await connectionLock.WaitAsync();
        try
        {
            await StopAsyncInternal();
        }
        finally
        {
            connectionLock.Release();
        }
    }

    private async Task StopAsyncInternal()
    {
        if (cancellationTokenSource != null)
        {
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch
            {
                 /* Ignore */
            }
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        if (client != null)
        {
            if (client.State == WebSocketState.Open)
            {
                try
                {
                    using var timeoutCts = new CancellationTokenSource(500);
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeoutCts.Token);
                }
                catch { /* Ignore */ }
            }
            client.Dispose();
            client = null;
        }
    }

    public void Dispose()
    {
        _ = StopAsync();
        connectionLock.Dispose();
    }
}
