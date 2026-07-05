using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using NativeMeters.Nodes;
using NativeMeters.Nodes.Components;

namespace NativeMeters.Rendering;

public static class MeterComponentInteractions
{
    public static void ApplyClickthrough(NodeBase node, bool isClickthrough)
    {
        if (!isClickthrough || node is HeaderMenuButtonNode)
        {
            return;
        }

        RemoveInteractionFlags(node);

        if (node is ComponentNode componentNode)
        {
            RemoveInteractionFlags(componentNode.CollisionNode);
        }

        if (node is BackgroundTextNode textNode)
        {
            RemoveInteractionFlags(textNode.BackgroundNode);
            RemoveInteractionFlags(textNode.TextNode);
        }
    }

    private static void RemoveInteractionFlags(NodeBase node)
        => node.RemoveNodeFlags(
            NodeFlags.EmitsEvents,
            NodeFlags.RespondToMouse,
            NodeFlags.HasCollision,
            NodeFlags.Focusable
        );
}
