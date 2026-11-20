using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Components.ProgressBars;

internal sealed class ProgressBarBardArrowNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/JobHudBRD0.tex";
    private const float FrameWidth = 188;
    private const float FrameHeight = 34;
    private const float FrameLeftCap = 62;
    private const float FrameRightCap = 45;
    private const float StripLeft = 19;
    private const float StripRight = 18;
    private const float StripWidth = FrameWidth - StripLeft - StripRight;
    private const float StripHeight = 6;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags HiddenFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled;
    private const NodeFlags VisibleFlags = HiddenFlags | NodeFlags.Visible;

    public ProgressBarBardArrowNode(bool preserveEnds)
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.BrightAdditive;
        FlatColorMode = GaugeColorMode.Solid;

        var stripPosition = new Vector2(StripLeft, 14);
        var stripSize = new Vector2(StripWidth, StripHeight);
        var track = GetPart(1, 419, 150, StripHeight);
        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = HiddenFlags,
            DrawFlags = GaugeDrawFlags,
            Position = stripPosition,
            Size = stripSize,
            WrapMode = WrapMode.Stretch,
        };
        BackdropImageNode.AddPart([track]);

        StaticBackdropNode = CreateSlicedLayer(13, track, stripPosition, 2, GaugeDrawFlags, VisibleFlags);
        StaticBackdropNode.Size = stripSize;
        MainFillNode = CreateSlicedLayer(11, GetPart(59, 427, 92, StripHeight), stripPosition, 2, GaugeDrawFlags, VisibleFlags);
        MainFillNode.Size = stripSize;

        BorderNode = CreateSlicedLayer(14, GetPart(1, 366, FrameWidth, FrameHeight), Vector2.Zero, FrameLeftCap,
            GaugeDrawFlags, VisibleFlags);
        BorderNode.RightOffset = FrameRightCap;

        InitializeGauge(new Vector2(FrameWidth, FrameHeight), StripWidth,
            minimumUnscaledWidth: preserveEnds ? FrameLeftCap + FrameRightCap + 1 : 0);
    }

    private static Part GetPart(float u, float v, float width, float height)
        => new()
        {
            TexturePath = TexturePath,
            TextureCoordinates = new Vector2(u, v),
            Size = new Vector2(width, height),
        };
}
