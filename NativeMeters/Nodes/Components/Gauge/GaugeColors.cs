using System;
using System.Numerics;
using KamiToolKit.BaseTypes;

namespace NativeMeters.Nodes.Components.Gauge;

internal static class GaugeColors
{
    private const float BrightTintTextureWeight = 0.12f;
    private const float BrightTintColorWeight = 0.90f;
    private const float BrightTintColorFloor = 0.03f;

    public static Vector4 Get(NodeBase node, GaugeColorMode colorMode)
        => colorMode switch
        {
            GaugeColorMode.Additive => new Vector4(node.AddColor.X, node.AddColor.Y, node.AddColor.Z, node.Color.W),
            GaugeColorMode.BrightAdditive => new Vector4(
                Math.Max(0.0f, (node.AddColor.X - BrightTintColorFloor) / BrightTintColorWeight),
                Math.Max(0.0f, (node.AddColor.Y - BrightTintColorFloor) / BrightTintColorWeight),
                Math.Max(0.0f, (node.AddColor.Z - BrightTintColorFloor) / BrightTintColorWeight),
                node.Color.W),
            GaugeColorMode.Multiply => new Vector4(node.MultiplyColor.X, node.MultiplyColor.Y, node.MultiplyColor.Z, node.Color.W),
            GaugeColorMode.TextureAlpha => new Vector4(1.0f, 1.0f, 1.0f, node.Color.W),
            _ => node.Color,
        };

    public static void Apply(NodeBase node, Vector4 value, GaugeColorMode colorMode, Vector3? additiveMultiplyColor = null)
    {
        if (colorMode == GaugeColorMode.Additive)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = additiveMultiplyColor ?? Vector3.One;
            node.AddColor = new Vector3(value.X, value.Y, value.Z);
            return;
        }

        if (colorMode == GaugeColorMode.BrightAdditive)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = new Vector3(BrightTintTextureWeight);
            node.AddColor = Vector3.Clamp(
                new Vector3(value.X, value.Y, value.Z) * BrightTintColorWeight + new Vector3(BrightTintColorFloor),
                Vector3.Zero,
                Vector3.One);
            return;
        }

        if (colorMode == GaugeColorMode.TextureAlpha)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = Vector3.One;
            node.AddColor = Vector3.Zero;
            return;
        }

        if (colorMode == GaugeColorMode.Multiply)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = new Vector3(value.X, value.Y, value.Z);
            node.AddColor = Vector3.Zero;
            return;
        }

        node.Color = value;
        node.MultiplyColor = Vector3.One;
        node.AddColor = Vector3.Zero;
    }
}
