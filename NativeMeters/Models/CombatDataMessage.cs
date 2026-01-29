using System;
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

    [JsonPropertyName("isActive")]
    public string IsActive { get; set; }

    public bool IsEncounterActive() => IsActive?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true;

    public bool IsSameEncounter(CombatDataMessage? other)
    {
        if (other == null) return false;

        return Encounter?.Title == other.Encounter?.Title &&
               Encounter?.Duration == other.Encounter?.Duration &&
               Encounter?.ENCDPS == other.Encounter?.ENCDPS &&
               Combatant?.Count == other.Combatant?.Count;
    }
}
