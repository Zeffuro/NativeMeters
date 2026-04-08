using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Visibility;

public sealed class VisibilityScrollingAreaNode : ScrollingListNode
{
    public VisibilityScrollingAreaNode()
    {
        ItemSpacing = 10;
        AddNode(new VisibilityConfigurationNode());
    }
}

