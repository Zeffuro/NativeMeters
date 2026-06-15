using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using AtkNodeFlags = FFXIVClientStructs.FFXIV.Component.GUI.NodeFlags;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class NativeTabbedVerticalListNode : ResNode, ILayoutListNode
{
    private readonly List<TabbedNodeEntry> nodeList = [];
    private bool suppressRecalculateLayout;

    public int NavIndex { get; set; }
    public int NavLeft { get; set; }
    public int NavRight { get; set; }
    public float TabSize { get; set; } = 18.0f;
    public bool FitWidth { get; set; }
    public bool FitContents { get; set; } = true;
    public int TabStep { get; set; }
    public float ItemSpacing { get; set; }
    public float FirstItemSpacing { get; set; }

    public IReadOnlyList<NodeBase> Nodes => nodeList.Select(entry => entry.Node).ToList();

    public bool ClipListContents
    {
        get => NodeFlags.HasFlag(AtkNodeFlags.Clip);
        set
        {
            if (value)
            {
                AddNodeFlags(AtkNodeFlags.Clip);
            }
            else
            {
                RemoveNodeFlags(AtkNodeFlags.Clip);
            }
        }
    }

    public ICollection<NodeBase> InitialNodes
    {
        set => AddNode(value);
    }

    public IEnumerable<T> GetNodes<T>() where T : NodeBase
        => nodeList.Select(entry => entry.Node).OfType<T>();

    public void RecalculateLayout()
    {
        if (suppressRecalculateLayout) return;

        var startY = FirstItemSpacing;
        var hasVisibleNode = false;

        foreach (var (node, tab) in nodeList)
        {
            if (!node.IsVisible) continue;

            node.Y = startY;
            node.X = tab * TabSize;

            if (FitWidth)
            {
                node.Width = Math.Max(0.0f, Width - node.X - ItemSpacing);
            }

            LayoutRecalculation.RecalculateForMeasurement(node);

            startY += node.Height + ItemSpacing;
            hasVisibleNode = true;
        }

        if (FitContents)
        {
            Height = hasVisibleNode ? startY + ItemSpacing : FirstItemSpacing;
        }
    }

    public void AddTab(int tabAmount = 1)
        => TabStep += tabAmount;

    public void SubtractTab(int tabAmount = 1)
        => TabStep -= tabAmount;

    public void AddNode(int tabIndex, IEnumerable<NodeBase> nodes)
    {
        suppressRecalculateLayout = true;
        try
        {
            foreach (var node in nodes)
            {
                AddNode(tabIndex, node);
            }
        }
        finally
        {
            suppressRecalculateLayout = false;
        }

        RecalculateLayout();
    }

    public void AddNode(int tabIndex, NodeBase node)
    {
        nodeList.Add(new TabbedNodeEntry(node, tabIndex + TabStep));
        node.AttachNode(this);

        if (!suppressRecalculateLayout)
        {
            RecalculateLayout();
        }
    }

    public void AddNode(NodeBase? node)
    {
        if (node is null) return;

        AddNode(0, node);
    }

    public void AddNode(IEnumerable<NodeBase> nodes)
        => AddNode(0, nodes);

    public void RemoveNode(IEnumerable<NodeBase> items)
    {
        suppressRecalculateLayout = true;
        try
        {
            foreach (var node in items)
            {
                RemoveNode(node);
            }
        }
        finally
        {
            suppressRecalculateLayout = false;
        }

        RecalculateLayout();
    }

    public void RemoveNode(NodeBase node)
    {
        var removed = nodeList.RemoveAll(entry => ReferenceEquals(entry.Node, node)) > 0;
        if (!removed) return;

        node.Dispose();
        RecalculateLayout();
    }

    public void AddDummy(float size = 0.0f)
        => AddNode(new ResNode { Size = new Vector2(size, size) });

    public void Clear()
    {
        foreach (var entry in nodeList)
        {
            entry.Node.Dispose();
        }

        nodeList.Clear();
        RecalculateLayout();
    }

    public void ReorderNodes(Comparison<NodeBase> comparison)
    {
        nodeList.Sort((left, right) => comparison(left.Node, right.Node));
        RecalculateLayout();
    }

    private sealed record TabbedNodeEntry(NodeBase Node, int Tab);
}
