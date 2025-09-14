using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NativeMeters.Models;

namespace NativeMeters.Services;

public class TestMeterService : MeterServiceBase, IDisposable
{
    private readonly Timer timer;
    private bool disposed;
    private readonly List<Combatant> fixedCombatants;

    public override event Action? CombatDataUpdated;

    public TestMeterService()
    {
        fixedCombatants = FakeCombatantFactory.CreateFixedCombatants(8);
        timer = new Timer(GenerateFakeData, null, 0, 1000); // every one second
    }

    private void GenerateFakeData(object? state)
    {
        var combatants = fixedCombatants
            .Select((c, i) => {
                var newCombatant = FakeCombatantFactory.CreateFakeCombatant(c.Name, i + 1);
                newCombatant.Job = c.Job;
                return newCombatant;
            })
            .ToDictionary(c => c.Name, c => c);

        CombatData = new CombatDataMessage
        {
            Type = "test",
            Encounter = FakeEncounterFactory.CreateFakeEncounter(combatants.Values),
            Combatant = combatants
        };

        CombatDataUpdated?.Invoke();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        timer.Dispose();
        CombatData = null;
    }
    public override bool IsConnected => true;
}