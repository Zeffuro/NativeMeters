using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.Components.ProgressBars;

public unsafe class ProgressBarCastGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/Parameter_Gauge.tex";
    private const float NativeWidth = GaugeGeometry.CastWidth;
    private const float NativeHeight = GaugeGeometry.CastHeight;
    private const float TrackCapWidth = GaugeGeometry.CastTrackCapWidth;
    private static readonly Vector3 NativeCastFillMultiplyColor = new(0.9f, 0.75f, 0.75f);
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    public ProgressBarCastGaugeNode() : this(false) { }

    internal ProgressBarCastGaugeNode(bool preserveEnds)
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.Additive;

        var parts = CreateParts();

        BackdropImageNode = CreateStockImage(12, parts, new Vector2(NativeWidth, NativeHeight),
            preserveEnds ? VisibleGaugeNodeFlags & ~NodeFlags.Visible : VisibleGaugeNodeFlags,
            GaugeDrawFlags, 5);

        MainFillNode = CreateFill(parts);

        if (preserveEnds)
        {
            StaticBackdropNode = CreateTrackLayer(14, 100.0f);
            BorderNode = CreateTrackLayer(13, 0.0f);
        }
        else
        {
            BorderImageNode = CreateStockImage(13, parts, new Vector2(NativeWidth, NativeHeight),
                VisibleGaugeNodeFlags, GaugeDrawFlags, 0);
        }

        InitializeGauge(new Vector2(NativeWidth, NativeHeight), minimumUnscaledWidth: preserveEnds ? TrackCapWidth * 2 : 0);
    }

    private static NineGridNode CreateTrackLayer(uint nodeId, float textureY)
        => CreateSlicedLayer(nodeId, new Part
        {
            TexturePath = TexturePath,
            TextureCoordinates = new Vector2(0, textureY),
            Size = new Vector2(NativeWidth, NativeHeight),
        }, Vector2.Zero, TrackCapWidth, GaugeDrawFlags, VisibleGaugeNodeFlags);

    private static NineGridNode CreateFill(Part[] parts)
    {
        var node = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 2,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = new Vector2(GaugeGeometry.CastFillInset, 0.0f),
            LeftOffset = 7.0f,
            RightOffset = 7.0f,
            MultiplyColor = NativeCastFillMultiplyColor,
            PartsRenderType = 36,
            Parts = parts,
        };
        node.Node->PartsList->Id = 1;
        return node;
    }

    private static Part[] CreateParts()
        => [
            new()
            {
                Id = 0,
                TexturePath = TexturePath,
                TextureCoordinates = Vector2.Zero,
                Size = new Vector2(NativeWidth, NativeHeight),
            },
            new()
            {
                Id = 1,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 20.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
            new()
            {
                Id = 2,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 40.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
            new()
            {
                Id = 3,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 60.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
            new()
            {
                Id = 4,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 80.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
            new()
            {
                Id = 5,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 100.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
        ];
}
