using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Configuration.General;

namespace NativeMeters.Nodes.Configuration.Connection;

public sealed class ConnectionScrollingAreaNode : ScrollingListNode
{

    public ConnectionScrollingAreaNode()
    {
        ItemSpacing = 10;

        var internalParserNode = new InternalParserConfigurationNode
        {
            IsVisible = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal,
        };

        var connectionNode = new ConnectionConfigurationNode
        {
            OnConnectionTypeChanged = type =>
            {
                internalParserNode.IsVisible = type == ConnectionType.Internal;
                RecalculateLayout();
            }
        };

        AddNode(connectionNode);
        AddNode(internalParserNode);
    }
}
