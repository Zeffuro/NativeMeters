using System.Numerics;
using KamiToolKit.BaseTypes;

namespace NativeMeters.Nodes.Configuration;

internal static class ConfigurationLayout
{
    public static void SetSize(NodeBase node, Vector2 size)
    {
        var nativeSize = new Vector2((ushort)size.X, (ushort)size.Y);
        if (node.Size != nativeSize) node.Size = size;
    }

    public static void SetWidth(NodeBase node, float width)
    {
        if (node.Width != (ushort)width) node.Width = width;
    }
}
