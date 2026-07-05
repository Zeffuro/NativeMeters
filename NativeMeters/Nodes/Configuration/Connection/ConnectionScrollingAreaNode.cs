using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Nodes.Configuration;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Connection;

public sealed class ConnectionScrollingAreaNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;

    public int TabBarNavIndex { get; set; } = 2;

    public ConnectionScrollingAreaNode()
    {
        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;
        ContentNode.FitWidth = true;

        var internalParserNode = new InternalParserConfigurationNode
        {
            IsVisible = System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal,
        };

        var connectionNode = new ConnectionConfigurationNode
        {
            OnConnectionTypeChanged = type =>
            {
                internalParserNode.IsVisible = type == ConnectionType.Internal;
                RecalculateSizes();
            }
        };

        ContentNode.AddNode([connectionNode, internalParserNode]);
        RecalculateConfigurationLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        RecalculateConfigurationLayout();
    }

    private void RecalculateConfigurationLayout()
    {
        ConfigurationNavigation.Apply(ContentNode, FirstContentNavIndex, TabBarNavIndex, TabBarNavIndex);
    }
}
