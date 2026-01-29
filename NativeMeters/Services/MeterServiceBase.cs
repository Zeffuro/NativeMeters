using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Models;

namespace NativeMeters.Services;

public abstract class MeterServiceBase : IMeterService
{
    protected CombatDataMessage? CombatData { get; set; }
    protected CombatDataMessage? LastEvent { get; set; }

    protected List<CombatDataMessage> EncounterHistory { get; } = new();
    protected int SelectedEncounterIndex { get; set; } = -1;
    protected int MaxHistorySize => System.Config?.General?.MaxEncounterHistory ?? 10;
    public virtual event Action? CombatDataUpdated;

    protected virtual void InvokeCombatDataUpdated()
    {
        CombatDataUpdated?.Invoke();
    }

    protected void ArchiveCurrentEncounter()
    {
        if (CombatData == null) return;

        if (EncounterHistory.Count > 0 && CombatData.IsSameEncounter(EncounterHistory.FirstOrDefault()))
            return;

        EncounterHistory.Insert(0, CombatData);

        while (EncounterHistory.Count > MaxHistorySize)
        {
            EncounterHistory.RemoveAt(EncounterHistory.Count - 1);
        }
    }

    protected void HandleNewCombatData(CombatDataMessage? newData)
    {
        if (newData?.Encounter == null || newData.Combatant == null || newData.Combatant.Count == 0)
            return;

        if (newData.IsSameEncounter(LastEvent))
            return;

        if (System.Config.General.EnableEncounterHistory &&
            !newData.IsEncounterActive() &&
            !newData.IsSameEncounter(EncounterHistory.FirstOrDefault()))
        {
            ArchiveCurrentEncounter();
        }

        if (System.Config.General.AutoSwitchToLiveEncounter &&
            IsViewingHistory &&
            newData.IsEncounterActive() &&
            IsNewEncounter(newData))
        {
            SelectLiveEncounter();
        }

        LastEvent = CombatData;
        CombatData = newData;
    }

    private bool IsNewEncounter(CombatDataMessage newData)
    {
        if (CombatData?.Encounter?.Title != newData.Encounter?.Title)
            return true;

        if (CombatData?.Encounter?.Duration > newData.Encounter?.Duration)
            return true;

        return false;
    }

    public virtual CombatDataMessage? GetViewedCombatData()
    {
        if (SelectedEncounterIndex < 0 || SelectedEncounterIndex >= EncounterHistory.Count)
            return CombatData;
        return EncounterHistory[SelectedEncounterIndex];
    }

    public virtual IEnumerable<Combatant> GetCombatants()
        => GetViewedCombatData()?.Combatant?.Values ?? Enumerable.Empty<Combatant>();

    public virtual Combatant? GetCombatant(string name)
        => GetViewedCombatData()?.Combatant?.GetValueOrDefault(name);

    public virtual Encounter? GetEncounter()
        => GetViewedCombatData()?.Encounter;

    public void SelectEncounter(int index)
    {
        SelectedEncounterIndex = index;
        InvokeCombatDataUpdated();
    }

    public void ClearHistory()
    {
        EncounterHistory.Clear();
        if (IsViewingHistory)
        {
            SelectLiveEncounter();
        }
    }

    public void SelectLiveEncounter() => SelectEncounter(-1);

    public IReadOnlyList<CombatDataMessage> GetEncounterHistory() => EncounterHistory;

    public bool IsViewingHistory => SelectedEncounterIndex >= 0;
    public int SelectedHistoryIndex => SelectedEncounterIndex;

    public double GetMaxCombatantStat(Func<Combatant, double> selector)
        => GetCombatants().Any() ? GetCombatants().Max(selector) : 1.0;

    public virtual bool HasCombatData()
        => GetViewedCombatData() != null;

    public virtual bool IsConnected => false;
}
