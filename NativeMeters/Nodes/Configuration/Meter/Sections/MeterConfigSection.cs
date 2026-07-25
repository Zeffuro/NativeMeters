using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public abstract class MeterConfigSection : CollapsingHeaderNode
{
    private const float HeaderHeight = 28.0f;
    protected const float ChildIndent = 8.0f;

    protected readonly Func<MeterSettings> GetMeterSettings;
    private readonly TabbedVerticalListNode contentNode;

    protected MeterSettings Settings => GetMeterSettings();
    protected float ContentItemSpacing
    {
        get => contentNode.ItemSpacing;
        set => contentNode.ItemSpacing = value;
    }

    public bool IsInitialized { get; set; }
    public float TabSize
    {
        get => contentNode.TabSize;
        set => contentNode.TabSize = value;
    }

    public int TabStep
    {
        get => contentNode.TabStep;
        set => contentNode.TabStep = value;
    }

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;

        contentNode = new TabbedVerticalListNode
        {
            FitContents = true,
            FitWidth = true,
            TabSize = 18.0f,
        };

        base.AddNode(contentNode);

        IsCollapsed = true;
        IsInitialized = false;
        FitWidth = false;
    }

    public abstract void Refresh();

    protected void AddTab(int tabAmount)
        => contentNode.AddTab(tabAmount);

    protected void SubtractTab(int tabAmount)
        => contentNode.SubtractTab(tabAmount);

    protected void AddNode(int tabIndex, NodeBase? node)
    {
        if (node == null) return;

        contentNode.AddNode(tabIndex, node);
        RecalculateSectionLayout();
    }

    protected void AddNode(int tabIndex, IEnumerable<NodeBase> nodes)
    {
        contentNode.AddNode(tabIndex, nodes);
        RecalculateSectionLayout();
    }

    public override void AddNode(NodeBase? node)
    {
        AddNode(0, node);
    }

    public override void AddNode(IEnumerable<NodeBase> nodes)
    {
        contentNode.AddNode(nodes);
        RecalculateSectionLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        ApplyContentLayout();
    }

    protected override void OnRecalculateLayout()
    {
        if (contentNode == null)
        {
            Height = HeaderHeight;
            return;
        }

        ApplyContentLayout();

        if (IsCollapsed)
        {
            contentNode.IsVisible = false;
            SetDirectContentRowsVisible(false);
            Height = HeaderHeight;
            return;
        }

        contentNode.IsVisible = true;
        SetDirectContentRowsVisible(true);
        contentNode.RecalculateLayout();
        contentNode.Position = new Vector2(ChildIndent, HeaderHeight + FirstItemSpacing);

        Height = contentNode.Y + contentNode.Height + ItemSpacing;
    }

    protected void RecalculateSectionLayout()
        => RecalculateLayout();

    private void ApplyContentLayout()
    {
        if (contentNode == null) return;

        contentNode.X = ChildIndent;
        contentNode.Width = Math.Max(0.0f, Width - ChildIndent);
    }

    private void SetDirectContentRowsVisible(bool isVisible)
    {
        foreach (var node in contentNode.Nodes)
        {
            node.IsVisible = isVisible;

            if (node is ILayoutListNode layoutNode)
            {
                foreach (var childNode in layoutNode.Nodes)
                {
                    childNode.IsVisible = isVisible;
                }
            }
        }
    }
}
