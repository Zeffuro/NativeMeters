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
    private bool enabled;
    private DateTime lastEmit = DateTime.MinValue;
    private const int EmitIntervalMs = 500;
    private uint territoryId;

    public bool IsDisposed => disposed;

    public override bool IsConnected => enabled && !disposed;
    public ulong UnattributedDotDamage => networkParser.UnattributedDotDamage;

    internal string StartCapture()
    {
        if (!System.Config.General.DebugEnabled) throw new InvalidOperationException("Enable Debug Mode before capturing.");
        if (!IsConnected) throw new InvalidOperationException("Enable the internal parser before capturing.");
        if (networkParser.Capture.IsActive) return networkParser.Capture.FilePath!;

        var path = networkParser.Capture.Start();
        networkParser.Capture.Record("CaptureStart", new
        {
            Version = typeof(InternalMeterService).Assembly.GetName().Version?.ToString(),
            BuildId = typeof(InternalMeterService).Module.ModuleVersionId,
            LocalPlayerId = Service.ObjectTable.LocalPlayer?.GameObjectId,
            LocalPlayerName = Service.ObjectTable.LocalPlayer?.Name.TextValue,
        });
        RecordMeterSnapshot();
        networkParser.Capture.Flush();
        if (!networkParser.Capture.IsActive) throw new InvalidOperationException("Could not start the capture. Check the plugin log.");

        return path;
    }

    internal string? StopCapture()
    {
        if (!networkParser.Capture.IsActive) return null;

        networkParser.ProcessPendingTicks(true);
        RecordMeterSnapshot();
        networkParser.Capture.Dispose();
        return networkParser.Capture.FilePath;
    }

    public void Enable()
    {
        if (disposed || enabled) return;

        enabled = true;
        territoryId = Service.ClientState.TerritoryType;
        networkParser.OnActionResult += combatTracker.HandleActionResult;
        networkParser.OnActorDeath += combatTracker.HandleDeath;
        networkParser.Enable();

        Service.Framework.Update += OnFrameworkTick;
        Service.NotificationManager.Success("Internal parser enabled (experimental).");
    }

    private void OnFrameworkTick(object? _)
    {
        if (disposed) return;

        if (!System.Config.General.DebugEnabled && networkParser.Capture.IsActive) StopCapture();

        if (territoryId != Service.ClientState.TerritoryType)
        {
            territoryId = Service.ClientState.TerritoryType;
            networkParser.ResetTracking();
        }

        networkParser.ProcessPendingTicks();
        combatTracker.UpdateCombatState();

        if (combatTracker.DidEncounterJustEnd)
        {
            networkParser.ProcessPendingTicks(true);

            if (System.Config.General.EnableEncounterHistory)
                ArchiveCurrentEncounter();
        }

        if ((DateTime.Now - lastEmit).TotalMilliseconds < EmitIntervalMs) return;

        lastEmit = DateTime.Now;
        networkParser.UpdateTracking();

        if (combatTracker.HasData) UpdateCombatData();

        RecordMeterSnapshot();
        networkParser.Capture.Flush();
    }

    private void RecordMeterSnapshot()
    {
        if (!networkParser.Capture.IsActive) return;

        var combatants = combatTracker.GetCombatants();
        var encounter = combatTracker.BuildEncounter(combatants.Values);
        networkParser.Capture.Record("Meter", new
        {
            TerritoryId = Service.ClientState.TerritoryType,
            Settings = System.Config.InternalParser,
            combatTracker.IsInCombat,
            DurationSeconds = encounter.Duration.TotalSeconds,
            encounter.Damage,
            UnattributedDotDamage,
            Actors = combatants.Select(pair => new { Name = pair.Key, pair.Value.Damage, pair.Value.Encdps }).ToArray(),
        });
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
        base.ResetLocalData();
    }

    public override void ResetLocalData()
    {
        combatTracker.Reset();
        networkParser.ResetTracking();
        base.ResetLocalData();
    }

    public override void EndEncounter()
    {
        networkParser.ProcessPendingTicks(true);

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

        StopCapture();
        var wasEnabled = enabled;

        disposed = true;
        enabled = false;

        if (wasEnabled)
        {
            Service.Framework.Update -= OnFrameworkTick;
            networkParser.OnActionResult -= combatTracker.HandleActionResult;
            networkParser.OnActorDeath -= combatTracker.HandleDeath;
        }

        networkParser.Dispose();
        combatTracker.Reset();
        CombatData = null;
    }
}
