using KamiToolKit.BaseTypes.ComponentNode;

namespace NativeMeters.Extensions;

internal static class ComponentNodeExtensions
{
    public static void DisableCollisionNode(this ComponentNode node)
        => node.CollisionNode.NodeFlags = 0;
}
