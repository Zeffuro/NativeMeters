using System;
using NativeMeters.Configuration;

namespace NativeMeters.Addons;

public class MeterWrapper(MeterSettings meterSettings)
{
    public MeterSettings MeterSettings { get; } = meterSettings;

    public string GetLabel() => string.IsNullOrWhiteSpace(MeterSettings.Name) ? "Unnamed Meter" : MeterSettings.Name;

    public string GetSubLabel()
    {
        var status = MeterSettings.IsEnabled ? "✓ Enabled" : " Disabled";
        return $"{status} ({MeterSettings.StatToTrack})";
    }

    public uint? GetId() => null;

    public uint? GetIconId() => 0;

    public int Compare(MeterWrapper other, string sortingMode)
    {
        return string.Compare(MeterSettings.Name, other.MeterSettings.Name, StringComparison.OrdinalIgnoreCase);
    }
}