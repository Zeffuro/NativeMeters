using System;
using NativeMeters.Configuration;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Meter;

public abstract class MeterConfigSection : CategoryNode
{
    protected readonly Func<MeterSettings> GetMeterSettings;
    protected MeterSettings Settings => GetMeterSettings();

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;
        HeaderHeight = 32.0f;
        FontSize = 14;
        NestingIndent = 0.0f;
    }

    public abstract void Refresh();
}