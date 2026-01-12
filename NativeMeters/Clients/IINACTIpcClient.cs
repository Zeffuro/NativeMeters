using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dalamud.Interface.ImGuiNotification;
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

    public event Action? OnConnected;

    public IINACTIpcClient()
    {
        var subscriptionReceiver = Service.PluginInterface.GetIpcProvider<JObject, bool>(SubscriptionName);
        subscriptionReceiver.RegisterFunc(HandleJObject);
    }

    public void Subscribe()
    {
        try
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

            OnConnected?.Invoke();

            var subscription = new SubscriptionRequest();
            var jsonNode = JsonNode.Parse(JsonSerializer.Serialize(subscription));
            var jObject = JObject.Parse(jsonNode?.ToJsonString() ?? string.Empty);
            Service.PluginInterface.GetIpcSubscriber<JObject, bool>(ProviderEditEndpoint).InvokeAction(jObject);
        }
        catch (Exception ex)
        {
            if (System.Config.ConnectionSettings.LogConnectionErrors) Service.Logger.Debug($"IINACT Subscribe failed (IINACT likely not running): {ex.Message}");
        }
    }

    public void Unsubscribe()
    {
        try
        {
            Service.PluginInterface.GetIpcSubscriber<string, bool>(UnsubscribeEndpoint).InvokeFunc(SubscriptionName);
        }
        catch (Exception ex)
        {
            if (System.Config.ConnectionSettings.LogConnectionErrors) Service.Logger.Debug($"IINACT Unsubscribe failed: {ex.Message}");
        }
    }

    private bool HandleJObject(JObject json)
    {
        System.MeterService.EnqueueIpcMessage(json.ToString());
        return true;
    }

    public bool IsConnected
    {
        get
        {
            try
            {
                return Service.PluginInterface.GetIpcSubscriber<bool>(ListeningEndpoint).InvokeFunc();
            }
            catch
            {
                return false;
            }
        }
    }
}