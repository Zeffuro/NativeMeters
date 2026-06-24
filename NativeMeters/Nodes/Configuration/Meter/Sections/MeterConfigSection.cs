using System;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public abstract class MeterConfigSection : CollapsingHeaderNode
{
    protected const float ChildIndent = 18.0f;

    protected readonly Func<MeterSettings> GetMeterSettings;
    protected MeterSettings Settings => GetMeterSettings();
    public bool IsInitialized { get; set; }

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;

        IsCollapsed = true;
        IsInitialized = false;
        FitWidth = false;

        //base.AddNode(BodyNode);
    }

    public abstract void Refresh();

    protected void RecalculateSectionLayout()
    {
        RecalculateLayout();
    }
}
