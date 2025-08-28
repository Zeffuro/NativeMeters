using System.Text.Json.Serialization;

namespace NativeMeters.Models;

public class SubscriptionRequest
{
    [JsonPropertyName("call")]
    public string Call { get; set; } = "subscribe";

    [JsonPropertyName("events")]
    public string[] Events { get; set; } = ["CombatData"];
}