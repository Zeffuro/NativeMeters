using System.Collections.Generic;
using Newtonsoft.Json;

namespace NativeMeters.Models;

public class CombatDataMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("Encounter")]
    public Encounter Encounter { get; set; }
    [JsonProperty("Combatant")]
    public Dictionary<string, Combatant> Combatant { get; set; }
}