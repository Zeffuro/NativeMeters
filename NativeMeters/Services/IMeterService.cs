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

    // --- Management Methods ---
    void ClearMeter();
    void EndEncounter();
    void Reconnect();

    // --- History Methods ---
    IReadOnlyList<CombatDataMessage> GetEncounterHistory();
    void SelectEncounter(int index);
    void SelectLiveEncounter();
    bool IsViewingHistory { get; }
    int SelectedHistoryIndex { get; }
    void ClearHistory();
}
