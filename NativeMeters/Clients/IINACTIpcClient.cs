using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Plugin;
using NativeMeters.Models;
using NativeMeters.Services;
using Newtonsoft.Json.Linq;

namespace NativeMeters.Clients;

public class IINACTIpcClient
{
    private const string SubscriptionName = "NativeMeters.SubscriptionReceiver";
    private const string SubscribeEndpoint = "IINACT.CreateSubscriber";
    private const string ListeningEndpoint = "IINACT.Server.Listening";
    private const string UnsubscribeEndpoint = "IINACT.Unsubscribe";
    private const string ProviderEditEndpoint = "IINACT.IpcProvider." + SubscriptionName;

    public IINACTIpcClient()
    {
        var subscriptionReceiver = Service.PluginInterface.GetIpcProvider<JObject, bool>(SubscriptionName);
        subscriptionReceiver.RegisterFunc(HandleJObject);
    }

    public void Subscribe()
    {
        var isRunning = Service.PluginInterface.GetIpcSubscriber<bool>(ListeningEndpoint).InvokeFunc();
        if (!isRunning)
        {
            Service.Logger.Error("IINACT doesn't seem to be running.");
            return;
        }

        var result = Service.PluginInterface.GetIpcSubscriber<string, bool>(SubscribeEndpoint).InvokeFunc(SubscriptionName);
        Service.Logger.Information(result ? "Subscribed to IINACT" : "Failed to subscribe to IINACT");
        if (!result) return;

        var subscription = new SubscriptionRequest();
        var jsonNode = JsonNode.Parse(JsonSerializer.Serialize(subscription));
        var jObject = JObject.Parse(jsonNode?.ToJsonString() ?? string.Empty);
        Service.PluginInterface.GetIpcSubscriber<JObject, bool>(ProviderEditEndpoint).InvokeAction(jObject);
    }

    public void Unsubscribe()
    {
        var result = Service.PluginInterface.GetIpcSubscriber<string, bool>(UnsubscribeEndpoint).InvokeFunc(SubscriptionName);
        Service.Logger.Information(result ? "Unsubscribed from IINACT" : "Failed to unsubscribe from IINACT");
    }

    private bool HandleJObject(JObject json)
    {
        Service.MeterService.EnqueueIpcMessage(json.ToString());
        return true;
    }
}