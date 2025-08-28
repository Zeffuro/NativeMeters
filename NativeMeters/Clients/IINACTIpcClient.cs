using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Clients;

public class IINACTIpcClient
{
    private const string DataEndpoint = "IINACT.SubscriptionReceiver";
    private const string SubscribeEndpoint = "IINACT.CreateSubscriber";
    private const string ListeningEndpoint = "IINACT.Server.Listening";
    private const string UnsubscribeEndpoint = "IINACT.Unsubscribe";
    private const string SubscriptionName = "NativeMeters.SubscriptionReceiver";
    private const string ProviderEditEndpoint = "IINACT.IpcProvider." + SubscriptionName;

    private readonly ICallGateProvider<object, bool> subscriptionReceiver;

    public IINACTIpcClient()
    {
        var callGateProvider = Service.PluginInterface.GetIpcProvider<object, bool>(DataEndpoint);
        callGateProvider.RegisterFunc(HandleObject);
        Service.PluginInterface.GetIpcProvider<string, bool>(SubscribeEndpoint);
        Service.PluginInterface.GetIpcProvider<bool>(ListeningEndpoint);
        Service.PluginInterface.GetIpcProvider<string, bool>(UnsubscribeEndpoint);
        subscriptionReceiver = Service.PluginInterface.GetIpcProvider<object, bool>(ProviderEditEndpoint);

        subscriptionReceiver.RegisterFunc(HandleObject);
    }

    public void Subscribe()
    {
        var isRunning = Service.PluginInterface.GetIpcSubscriber<bool>(ListeningEndpoint).InvokeFunc();
        if (!isRunning) Service.Logger.Error("IINACT doesn't seem to be running.");

        var connected = Service.PluginInterface.GetIpcSubscriber<string, bool>(SubscribeEndpoint).InvokeFunc(SubscriptionName);
        Service.Logger.Information(connected ? "Connected to IINACT IPC." : "Failed to connect to IINACT.");

        var subscription = new SubscriptionRequest();
        string jsonSubscription = JsonSerializer.Serialize(subscription);
        JsonNode? jsonNode = JsonNode.Parse(jsonSubscription);
        if(jsonNode is JsonObject jsonObject)
        {
            Service.PluginInterface.GetIpcSubscriber<object, bool>(SubscribeEndpoint).InvokeAction(jsonObject);
        }
    }

    public void Unsubscribe()
    {
        var result = Service.PluginInterface.GetIpcSubscriber<string, bool>(UnsubscribeEndpoint).InvokeFunc(SubscriptionName);
        Service.Logger.Information(result ? "Unsubscribed from IINACT" : "Failed to unsubscribe from IINACT");
    }

    private bool HandleObject(object data)
    {
        if (data is string json)
        {
            Service.MeterService.EnqueueIpcMessage(json);
            return true;
        }
        var str = data.ToString();
        if (string.IsNullOrEmpty(str)) return false;
        Service.MeterService.EnqueueIpcMessage(str);
        return true;
    }
}