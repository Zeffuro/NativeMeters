using System;
using System.Linq;
using System.Text.Json;
using NativeMeters.Models;
using NativeMeters.Services.Internal;

namespace NativeMeters.Services.Connections;

public class InternalCombatProcessor : IDisposable
{
    public event Action<string>? OnCombatDataJson;

    private readonly CombatTracker combatTracker = new();
    private readonly NetworkCombatParser networkParser = new();

    private bool disposed;
    private DateTime lastEmit = DateTime.MinValue;
    private const int EmitIntervalMs = 500;

    public void Enable()
    {
        networkParser.OnActionResult += combatTracker.HandleActionResult;
        networkParser.OnActorDeath += combatTracker.HandleDeath;
        networkParser.Enable();

        Service.Framework.Update += OnFrameworkTick;

        Service.Logger.Information("[Internal Parser] Enabled (experimental)");
    }

    private void OnFrameworkTick(object? _)
    {
        if (disposed) return;

        combatTracker.UpdateCombatState();
        networkParser.Tick();

        if ((DateTime.Now - lastEmit).TotalMilliseconds < EmitIntervalMs) return;
        lastEmit = DateTime.Now;

        if (!combatTracker.HasData) return;

        var json = BuildCombatDataJson();
        if (json != null)
            OnCombatDataJson?.Invoke(json);
    }

    private string? BuildCombatDataJson()
    {
        var combatants = combatTracker.GetCombatants();
        if (combatants.Count == 0) return null;

        var totalDamage = combatants.Values.Sum(c => c.Damage);
        if (totalDamage > 0)
        {
            foreach (var combatant in combatants.Values)
            {
                combatant.DamagePercent = combatant.Damage * 100.0 / totalDamage;
            }
        }

        var encounter = combatTracker.BuildEncounter(combatants.Values);

        var message = new CombatDataMessage
        {
            Type = "CombatData",
            Encounter = encounter,
            Combatant = combatants,
            IsActive = combatTracker.IsInCombat ? "true" : "false"
        };

        return JsonSerializer.Serialize(message);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        Service.Framework.Update -= OnFrameworkTick;
        networkParser.OnActionResult -= combatTracker.HandleActionResult;
        networkParser.OnActorDeath -= combatTracker.HandleDeath;
        networkParser.Dispose();
    }
}
