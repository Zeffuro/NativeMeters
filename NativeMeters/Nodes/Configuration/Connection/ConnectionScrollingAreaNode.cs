using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.General;

namespace NativeMeters.Nodes.Configuration.Connection;

public sealed class ConnectionScrollingAreaNode : ScrollingListNode
{
    public ConnectionScrollingAreaNode()
    {
        ItemSpacing = 10;

        AddNode(new ConnectionConfigurationNode());
    }
}