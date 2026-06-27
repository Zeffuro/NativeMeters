using System;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public abstract class MeterConfigSection : CollapsingHeaderNode
{
    protected const float ChildIndent = 8.0f;

    protected readonly Func<MeterSettings> GetMeterSettings;
    protected MeterSettings Settings => GetMeterSettings();
    public bool IsInitialized { get; set; }

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;

        IsCollapsed = true;
        IsInitialized = false;
        FitWidth = false;

    }

    public abstract void Refresh();

    public override void AddNode(NodeBase? node)
    {
        if (node != null) ApplyChildLayout(node);
        base.AddNode(node);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        foreach (var node in Nodes)
        {
            ApplyChildLayout(node);
        }
    }

    protected void RecalculateSectionLayout()
    {
        RecalculateLayout();
    }

    protected void ApplyChildLayout(NodeBase node)
    {
        node.X = ChildIndent;
        node.Width = Math.Max(0.0f, Width - ChildIndent);
    }
}
