using System;
using System.Threading;
using NativeMeters.Models;

namespace NativeMeters.Services;

public class TestMeterService : IDisposable, IMeterService
{
    private readonly Timer _timer;
    private bool _disposed;
    private CombatDataMessage? _currentCombatData;
    private readonly Random _random = new();

    public event Action? CombatDataUpdated;

    public TestMeterService()
    {
        _timer = new Timer(GenerateFakeData, null, 0, 2000); // every 2 seconds
    }

    public CombatDataMessage? CurrentCombatData => _currentCombatData;

    private void GenerateFakeData(object? state)
    {
        int count = _random.Next(2, 8);
        var combatants = FakeCombatantFactory.CreateFakeCombatants(count);

        _currentCombatData = new CombatDataMessage
        {
            Type = "test",
            Encounter = FakeEncounterFactory.CreateFakeEncounter(combatants.Values),
            Combatant = combatants
        };

        CombatDataUpdated?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Dispose();
        _currentCombatData = null;
    }
}