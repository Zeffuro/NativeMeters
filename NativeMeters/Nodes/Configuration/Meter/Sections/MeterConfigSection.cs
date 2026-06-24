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
    public VerticalListNode BodyNode { get; }
    public float BodyWidth => Math.Max(0.0f, Width - BodyNode.X);

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;

        IsCollapsed = true;
        IsInitialized = false;
        FitWidth = false;

        BodyNode = new VerticalListNode
        {
            X = ChildIndent,
            FitContents = true,
            FitWidth = true,
        };
        base.AddNode(BodyNode);
    }

    public abstract void Refresh();

    protected void RecalculateSectionLayout()
    {
        BodyNode.RecalculateLayout();
        RecalculateLayout();
    }
}
