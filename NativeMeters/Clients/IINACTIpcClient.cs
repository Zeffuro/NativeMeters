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
    private bool isSubscribed;

    public IINACTIpcClient()
    {
        var subscriptionReceiver = Service.PluginInterface.GetIpcProvider<JObject, bool>(SubscriptionName);
        subscriptionReceiver.RegisterFunc(HandleJObject);
    }

    public void Subscribe()
    {
        if (isSubscribed) return;

        try
        {
            if (!IsIINACTRunning)
            {
                Service.Logger.Debug("IINACT is not running, skipping subscribe.");
                return;
            }

            var result = Service.PluginInterface.GetIpcSubscriber<string, bool>(SubscribeEndpoint).InvokeFunc(SubscriptionName);
            if (!result) return;

            isSubscribed = true;
            OnConnected?.Invoke();

            var subscription = new SubscriptionRequest();
            var jsonNode = JsonNode.Parse(JsonSerializer.Serialize(subscription));
            var jObject = JObject.Parse(jsonNode?.ToJsonString() ?? string.Empty);
            Service.PluginInterface.GetIpcSubscriber<JObject, bool>(ProviderEditEndpoint).InvokeAction(jObject);

            Service.Logger.Information("Successfully subscribed to IINACT.");
        }
        catch (Exception ex)
        {
            isSubscribed = false;
            if (System.Config.ConnectionSettings.LogConnectionErrors)
                Service.Logger.Debug($"IINACT Subscribe failed: {ex.Message}");
        }
    }

    public void Unsubscribe()
    {
        if (!isSubscribed) return;

        try
        {
            if (IsIINACTRunning)
            {
                Service.PluginInterface.GetIpcSubscriber<string, bool>(UnsubscribeEndpoint).InvokeFunc(SubscriptionName);
            }
        }
        catch (Exception ex)
        {
            if (System.Config.ConnectionSettings.LogConnectionErrors)
                Service.Logger.Debug($"IINACT Unsubscribe failed: {ex.Message}");
        }
        finally
        {
            isSubscribed = false;
        }
    }

    private bool HandleJObject(JObject json)
    {
        System.MeterService.EnqueueIpcMessage(json.ToString());
        return true;
    }

    public bool IsIINACTRunning
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

    public bool IsConnected => isSubscribed && IsIINACTRunning;
}
