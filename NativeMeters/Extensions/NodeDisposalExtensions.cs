using System.Collections.Generic;
using Dalamud.Plugin.Services;
using KamiToolKit.BaseTypes;
using NativeMeters.Services;

namespace NativeMeters.Extensions;

internal static class NodeDisposalExtensions
{
    private static readonly object SyncRoot = new();
    private static readonly Queue<NodeBase> PendingNodes = new();
    private static readonly HashSet<NodeBase> PendingNodeSet = [];

    private static bool isFlushScheduled;

    public static void DisposeLater(this NodeBase? node)
    {
        if (node == null)
            return;

        node.IsVisible = false;

        lock (SyncRoot)
        {
            if (!PendingNodeSet.Add(node))
                return;

            PendingNodes.Enqueue(node);
            EnsureFlushScheduled();
        }
    }

    public static void DisposeValuesLater<TKey>(this IDictionary<TKey, NodeBase> dict)
    {
        foreach (var value in dict.Values)
        {
            value.DisposeLater();
        }

        dict.Clear();
    }

    public static void FlushPendingNodeDisposals()
    {
        List<NodeBase> nodes;

        lock (SyncRoot)
        {
            if (isFlushScheduled)
            {
                Service.Framework.Update -= FlushOnFrameworkUpdate;
                isFlushScheduled = false;
            }

            nodes = DrainPendingNodes();
        }

        DisposeNodes(nodes);
    }

    private static void EnsureFlushScheduled()
    {
        if (isFlushScheduled || Service.Framework.IsFrameworkUnloading)
            return;

        Service.Framework.Update += FlushOnFrameworkUpdate;
        isFlushScheduled = true;
    }

    private static void FlushOnFrameworkUpdate(IFramework framework)
    {
        List<NodeBase> nodes;

        lock (SyncRoot)
        {
            framework.Update -= FlushOnFrameworkUpdate;
            isFlushScheduled = false;
            nodes = DrainPendingNodes();
        }

        DisposeNodes(nodes);
    }

    private static List<NodeBase> DrainPendingNodes()
    {
        var nodes = new List<NodeBase>(PendingNodes.Count);

        while (PendingNodes.TryDequeue(out var node))
        {
            PendingNodeSet.Remove(node);
            nodes.Add(node);
        }

        return nodes;
    }

    private static void DisposeNodes(IEnumerable<NodeBase> nodes)
    {
        foreach (var node in nodes)
        {
            node.Dispose();
        }
    }
}
