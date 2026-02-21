using System;
using System.Collections.Generic;

namespace NativeMeters.Models.Breakdown;

public class EncounterSnapshot
{
    public required DateTime EndTime { get; init; }
    public required string EncounterName { get; init; }
    public required TimeSpan Duration { get; init; }
    public required Encounter Encounter { get; init; }
    public required List<Combatant> Combatants { get; init; }
}
