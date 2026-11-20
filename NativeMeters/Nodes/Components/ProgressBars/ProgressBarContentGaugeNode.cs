using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Components.ProgressBars;

internal sealed class ProgressBarContentGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/ContentGauge.tex";
    private const float NativeWidth = 240f;
    private const float NativeHeight = 22f;
    private const float FrameCapWidth = 44f;
    private const float TrackCapWidth = 38f;
    private const float FillInset = 35f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags HiddenFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled;
    private const NodeFlags VisibleFlags = HiddenFlags | NodeFlags.Visible;
    private static readonly Vector2 TrackPosition = new(0, 5);

    public ProgressBarContentGaugeNode(bool preserveEnds)
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.Additive;
        FlatColorMode = GaugeColorMode.Solid;

        BackdropImageNode = CreateImage(12, GetPart(22, 12), TrackPosition, !preserveEnds);
        MainFillNode = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = VisibleFlags,
            DrawFlags = GaugeDrawFlags,
            Position = TrackPosition,
            Size = new Vector2(NativeWidth, 12),
            Origin = new Vector2(FillInset, 0),
            LeftOffset = FillInset,
            RightOffset = FillInset,
            MultiplyColor = LegacyAdditiveTextureWeight,
            Parts = [GetPart(34, 12)],
        };

        var frame = GetPart(0, NativeHeight);
        if (preserveEnds)
        {
            StaticBackdropNode = CreateSlicedLayer(14, GetPart(22, 12), TrackPosition, TrackCapWidth,
                GaugeDrawFlags, VisibleFlags);
            BorderNode = CreateSlicedLayer(13, frame, Vector2.Zero, FrameCapWidth, GaugeDrawFlags, VisibleFlags);
        }
        else
        {
            BorderImageNode = CreateImage(13, frame, Vector2.Zero);
        }

        InitializeGauge(new Vector2(NativeWidth, NativeHeight), minimumUnscaledWidth: preserveEnds ? FrameCapWidth * 2 : 0);
    }

    private static ImageNode CreateImage(uint id, Part part, Vector2 position, bool visible = true)
    {
        var node = new ImageNode
        {
            NodeId = id,
            NodeFlags = visible ? VisibleFlags : HiddenFlags,
            DrawFlags = GaugeDrawFlags,
            Position = position,
            Size = part.Size,
            WrapMode = WrapMode.Stretch,
        };
        node.AddPart([part]);
        return node;
    }

    private static Part GetPart(float textureY, float height)
        => new()
        {
            TexturePath = TexturePath,
            TextureCoordinates = new Vector2(0, textureY),
            Size = new Vector2(NativeWidth, height),
        };
}
