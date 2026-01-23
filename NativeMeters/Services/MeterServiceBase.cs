using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Models;

namespace NativeMeters.Services;

public abstract class MeterServiceBase : IMeterService
{
    protected CombatDataMessage? CombatData { get; set; }

    public virtual event Action? CombatDataUpdated;

    protected virtual void InvokeCombatDataUpdated()
    {
        CombatDataUpdated?.Invoke();
    }

    public virtual IEnumerable<Combatant> GetCombatants()
        => CombatData?.Combatant?.Values ?? Enumerable.Empty<Combatant>();

    public virtual Combatant? GetCombatant(string name)
        => CombatData?.Combatant?.GetValueOrDefault(name);

    public virtual Encounter? GetEncounter()
        => CombatData?.Encounter;

    public double GetMaxCombatantStat(Func<Combatant, double> selector)
        => GetCombatants().Any() ? GetCombatants().Max(selector) : 1.0;

    public virtual bool HasCombatData()
        => CombatData != null;

    public virtual bool IsConnected => false;
}
