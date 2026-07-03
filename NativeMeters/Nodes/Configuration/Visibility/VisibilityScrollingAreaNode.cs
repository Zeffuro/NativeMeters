using KamiToolKit.Nodes;
using NativeMeters.Nodes.Configuration;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Visibility;

public sealed class VisibilityScrollingAreaNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;

    public int TabBarNavIndex { get; set; } = 5;

    public VisibilityScrollingAreaNode()
    {
        ReverseContentLayoutUpdate = true;

        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;
        ContentNode.AddNode(new VisibilityConfigurationNode());
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
