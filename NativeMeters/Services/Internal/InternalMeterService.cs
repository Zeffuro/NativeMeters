using System;
using System.Linq;
using NativeMeters.Extensions;
using NativeMeters.Models;

namespace NativeMeters.Services.Internal;

public class InternalMeterService : MeterServiceBase, IDisposable
{
    private readonly CombatTracker combatTracker = new();
    private readonly NetworkCombatParser networkParser = new();

    private bool disposed;
    private DateTime lastEmit = DateTime.MinValue;
    private const int EmitIntervalMs = 500;

    public override bool IsConnected => !disposed;

    public void Enable()
    {
        networkParser.OnActionResult += combatTracker.HandleActionResult;
        networkParser.OnActorDeath += combatTracker.HandleDeath;
        networkParser.Enable();

        Service.Framework.Update += OnFrameworkTick;
        Service.NotificationManager.Success("Internal parser enabled (experimental).");
    }

    private void OnFrameworkTick(object? _)
    {
        if (disposed) return;

        combatTracker.UpdateCombatState();

        if (combatTracker.DidEncounterJustEnd)
        {
            if (System.Config.General.EnableEncounterHistory)
                ArchiveCurrentEncounter();
        }

        if ((DateTime.Now - lastEmit).TotalMilliseconds < EmitIntervalMs) return;
        lastEmit = DateTime.Now;

        if (!combatTracker.HasData) return;

        UpdateCombatData();
    }

    private void UpdateCombatData()
    {
        var combatants = combatTracker.GetCombatants();
        if (combatants.Count == 0) return;

        var encounter = combatTracker.BuildEncounter(combatants.Values);

        CombatData = new CombatDataMessage
        {
            Type = "CombatData",
            Encounter = encounter,
            Combatant = combatants,
            IsActive = combatTracker.IsInCombat ? "true" : "false"
        };

        InvokeCombatDataUpdated();
    }

    public override void ClearMeter()
    {
        combatTracker.Reset();
        networkParser.ResetTracking();
        CombatData = null;
        InvokeCombatDataUpdated();
    }

    public override void EndEncounter()
    {
        if (System.Config.General.EnableEncounterHistory)
            ArchiveCurrentEncounter();

        combatTracker.ForceEndEncounter();
    }

    public override void Reconnect()
    {
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        Service.Framework.Update -= OnFrameworkTick;
        networkParser.OnActionResult -= combatTracker.HandleActionResult;
        networkParser.OnActorDeath -= combatTracker.HandleDeath;
        networkParser.Dispose();
        combatTracker.Reset();
        CombatData = null;
    }
}
