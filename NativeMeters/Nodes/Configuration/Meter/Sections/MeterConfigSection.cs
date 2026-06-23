using System;
using System.Collections.Generic;
using KamiToolKit.BaseTypes;
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
    }

    public abstract void Refresh();

    public override void AddNode(NodeBase? node)
    {
        if (node is null) return;

        ApplyChildLayout(node);
        base.AddNode(node);
    }

    public override void AddNode(IEnumerable<NodeBase> nodes)
    {
        foreach (var node in nodes)
        {
            AddNode(node);
        }
    }

    protected void RecalculateSectionLayout()
    {
        ApplyChildLayout();
        RecalculateLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ApplyChildLayout();
    }

    protected void ApplyChildLayout()
    {
        foreach (var node in Nodes)
        {
            ApplyChildLayout(node);
        }
    }

    protected void ApplyChildLayout(NodeBase node)
    {
        node.X = ChildIndent;
        node.Width = Math.Max(0.0f, Width - ChildIndent);
    }
}
