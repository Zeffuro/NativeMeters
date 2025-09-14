using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using NativeMeters.Config;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Clients;

public class WebSocketClient
{
    private readonly ClientWebSocket client = new();
    private readonly ConcurrentQueue<string> messageQueue = new();
    public event Action<string>? OnMessageReceived;

    private bool LogConnectionErrors => ConnectionConfig.Instance.LogConnectionErrors;

    private async Task ConnectAsync(Uri uri)
    {
        await client.ConnectAsync(uri, CancellationToken.None);
    }

    private async Task SendAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task<string> ReceiveDataAsync()
    {
        var buffer = new byte[4096];
        using var ms = new System.IO.MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await client.ReceiveAsync(buffer, CancellationToken.None);
            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public async Task StartAsync(Uri serverUri)
    {
        await ConnectAsync(serverUri);

        var subscription = new SubscriptionRequest();
        await SendAsync(JsonSerializer.Serialize(subscription));
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
            catch (Exception ex)
            {
                if (LogConnectionErrors)
                {
                    Service.Logger.Error($"WebSocket error: {ex}");
                }
            }
        });
    }

    public bool IsConnected => client.State == WebSocketState.Open;

    private async Task DisconnectAsync()
    {
        if (IsConnected)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
        }
    }

    public void Dispose()
    {
        if (client.State == WebSocketState.Open)
        {
            _ = DisconnectAsync();
        }
        client.Dispose();
    }
}