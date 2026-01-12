using System;
using KamiToolKit.Premade;
using NativeMeters.Configuration;

namespace NativeMeters.Addons;

public class MeterWrapper(MeterSettings meterSettings) : IInfoNodeData
{
    public MeterSettings MeterSettings { get; } = meterSettings;

    public string GetLabel()
    {
        return string.IsNullOrWhiteSpace(MeterSettings.Name) ? "Unnamed Meter" : MeterSettings.Name;
    }

    public string GetSubLabel()
    {
        var status = MeterSettings.IsEnabled ? "✓ Enabled" : " Disabled";
        var lockStatus = MeterSettings.IsLocked ? " |  Locked" : "";

        return $"{status}{lockStatus} ({MeterSettings.StatToTrack})";
    }

    public uint? GetId() => null;

    public uint? GetIconId() => 0;

    public string? GetTexturePath() => null;

    public int Compare(IInfoNodeData other, string sortingMode)
    {
        if (other is not MeterWrapper otherWrapper) return 0;

        return string.Compare(MeterSettings.Name, otherWrapper.MeterSettings.Name, StringComparison.OrdinalIgnoreCase);
    }
}