using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NativeMeters.Addons;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Configuration.Setup;
using NativeMeters.Localization;

namespace NativeMeters.Services;

internal sealed class MeterPlacementSession(SystemConfiguration config, MeterSettings meter)
{
    private SetupSampleFight? sample;
    private readonly Dictionary<Func<Combatant, double>, double> sampleMaxima = [];

    public MeterSettings Meter => meter;
    public Vector2 Position { get; set; } = meter.Position;
    public Vector2 Size { get; set; } = meter.Size;
    public bool DialogOpened { get; set; }
    public MeterPlacementOutcome? Outcome { get; private set; }
    public SetupSampleFight Sample => sample ??= new SetupSampleFight(Loc.Get("PlacementSampleTitle"));

    public bool AppliesTo(SystemConfiguration current)
        => ReferenceEquals(config, current) && config.General.IsEnabled && meter.IsEnabled && !meter.IsCollapsed
            && config.Meters.Contains(meter);

    public void RequestOutcome(MeterPlacementOutcome outcome)
        => Outcome ??= outcome;

    public void Cancel() => Outcome = MeterPlacementOutcome.Skip;

    public bool TryCommit(SystemConfiguration current)
    {
        if (!AppliesTo(current) || Outcome is not (MeterPlacementOutcome.Lock or MeterPlacementOutcome.KeepMovable))
            return false;

        meter.Position = Position;
        meter.Size = Size;
        meter.IsLocked = Outcome == MeterPlacementOutcome.Lock;
        return true;
    }

    public double GetSampleMaximum(Func<Combatant, double> selector)
    {
        if (sampleMaxima.TryGetValue(selector, out var maximum)) return maximum;
        maximum = Sample.Members.Max(selector);
        sampleMaxima.Add(selector, maximum);
        return maximum;
    }
}
