using System.Collections.Generic;
using Newtonsoft.Json;

namespace NativeMeters.Models;

public class CombatDataMessage
{
    [JsonProperty("type")]
    public required string Type { get; set; }
    [JsonProperty("Encounter")]
    public required Encounter Encounter { get; set; }
    [JsonProperty("Combatant")]
    public required Dictionary<string, Combatant> Combatant { get; set; }
}