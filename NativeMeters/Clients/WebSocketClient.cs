using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Clients;

public class WebSocketClient
{
    private ClientWebSocket? client;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly ConcurrentQueue<string> messageQueue = new();
    public event Action<string>? OnMessageReceived;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    private bool LogConnectionErrors => System.Config.ConnectionSettings.LogConnectionErrors;

    private async Task ConnectAsync(Uri uri)
    {
        await client?.ConnectAsync(uri, CancellationToken.None)!;
    }

    private async Task SendAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await client?.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None)!;
    }

    private async Task<string> ReceiveDataAsync()
    {
        var buffer = new byte[4096];
        using var ms = new global::System.IO.MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await client?.ReceiveAsync(buffer, CancellationToken.None)!;
            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public async Task StartAsync(Uri serverUri)
    {
        await StopAsync();

        client = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await ConnectAsync(serverUri);

            var subscription = new SubscriptionRequest();
            await SendAsync(JsonSerializer.Serialize(subscription));

            OnConnected?.Invoke();

            _ = Task.Run(async () =>
            {
                try
                {
                    while (IsConnected)
                    {
                        var message = await ReceiveDataAsync();
                        messageQueue.Enqueue(message);
                        OnMessageReceived?.Invoke(message);
                    }
                }
                catch
                {
                    OnDisconnected?.Invoke();
                }
            });
        } catch (Exception ex)
        {
            if (LogConnectionErrors) Service.Logger.Error($"WebSocket error: {ex}");
            OnDisconnected?.Invoke();
        }
    }

    public bool IsConnected => client?.State == WebSocketState.Open;

    public async Task StopAsync()
    {
        // Capture references to avoid race conditions during null-setting
        var tokenSource = cancellationTokenSource;
        var clientWebSocket = client;

        if (tokenSource != null)
        {
            try { await tokenSource.CancelAsync(); }
            catch { /* Ignore */ }
            tokenSource.Dispose();
            cancellationTokenSource = null;
        }

        if (clientWebSocket != null)
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    // Use a short timeout for the close handshake
                    using var timeoutCts = new CancellationTokenSource(1000);
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeoutCts.Token);
                }
                catch { /* Ignore */ }
            }
            clientWebSocket.Dispose();
            client = null;
        }
    }

    public void Dispose() => _ = StopAsync();
}