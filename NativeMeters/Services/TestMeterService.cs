using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NativeMeters.Models;

namespace NativeMeters.Services;

public class TestMeterService : MeterServiceBase, IDisposable
{
    private bool disposed;
    private readonly List<Combatant> fixedCombatants = FakeCombatantFactory.CreateFixedCombatants(20);
    private DateTime lastUpdate = DateTime.MinValue;

    public void Tick()
    {
        if ((DateTime.Now - lastUpdate).TotalMilliseconds < 1000) return;
        lastUpdate = DateTime.Now;

        GenerateFakeData();
    }

    private void GenerateFakeData()
    {
        var combatants = fixedCombatants
            .Select((combatant, i) => {
                var newCombatant = FakeCombatantFactory.CreateFakeCombatant(combatant.Name, i + 1);
                newCombatant.Job = combatant.Job;
                return newCombatant;
            })
            .ToDictionary(c => c.Name, c => c);

        CombatData = new CombatDataMessage
        {
            Type = "test",
            Encounter = FakeEncounterFactory.CreateFakeEncounter(combatants.Values),
            Combatant = combatants,
            IsActive = "true"
        };

        InvokeCombatDataUpdated();
    }

    public override void ClearMeter()
    {
        CombatData = null;
        InvokeCombatDataUpdated();
    }

    public override void EndEncounter()
    {
        if (CombatData != null)
        {
            CombatData.IsActive = "false";
            ArchiveCurrentEncounter();
        }
    }

    public override void Reconnect()
    {
        GenerateFakeData();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        CombatData = null;
    }
    public override bool IsConnected => true;
}
