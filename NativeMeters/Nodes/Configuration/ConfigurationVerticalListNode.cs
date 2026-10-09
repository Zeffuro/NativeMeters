using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration;

internal sealed class ConfigurationVerticalListNode : VerticalListNode, ILayoutListNode
{
    public new void RecalculateLayout()
    {
        foreach (var node in Nodes)
        {
            if (!node.IsVisible) continue;
            if (FitWidth) ConfigurationLayout.SetWidth(node, Width);
            if (node is ILayoutListNode layout) layout.RecalculateLayout();
        }

        base.OnRecalculateLayout();
        if (NavIndex != 0) base.OnRecalculateNavigation();
    }

    void ILayoutListNode.RecalculateLayout() => RecalculateLayout();
}
