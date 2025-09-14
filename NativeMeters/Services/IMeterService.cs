using System;
using System.Collections.Generic;
using NativeMeters.Models;

namespace NativeMeters.Services;

public interface IMeterService
{
    event Action? CombatDataUpdated;
    IEnumerable<Combatant> GetCombatants();
    Combatant? GetCombatant(string name);
    Encounter? GetEncounter();
    double GetMaxCombatantStat(Func<Combatant, double> selector);
    bool HasCombatData();
    bool IsConnected { get; }
}