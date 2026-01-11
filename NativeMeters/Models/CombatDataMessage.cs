using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NativeMeters.Models;

public class CombatDataMessage
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("Encounter")]
    public required Encounter Encounter { get; set; }
    [JsonPropertyName("Combatant")]
    public required Dictionary<string, Combatant> Combatant { get; set; }
}