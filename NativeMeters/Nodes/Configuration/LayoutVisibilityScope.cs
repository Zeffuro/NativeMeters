using System;
using System.Collections.Generic;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using KamiToolKit.Interfaces;

namespace NativeMeters.Nodes.Configuration;

internal sealed class LayoutVisibilityScope : IDisposable
{
    private const DrawFlags FullyClipped = (DrawFlags)0x40000;
    private List<NodeBase>? clippedRows;

    public LayoutVisibilityScope(NodeBase root) => Include(root);

    private void Include(NodeBase node)
    {
        if ((node.DrawFlags & FullyClipped) != 0)
        {
            (clippedRows ??= []).Add(node);
            node.DrawFlags &= ~FullyClipped;
        }

        if (node is ILayoutListNode layout)
        {
            foreach (var child in layout.Nodes)
                Include(child);
        }
    }

    public void Dispose()
    {
        if (clippedRows == null) return;

        foreach (var row in clippedRows)
            row.DrawFlags |= FullyClipped;
    }
}
