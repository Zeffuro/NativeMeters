using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Components.ProgressBars;

public unsafe class ProgressBarPartyListHpNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/PartyList_GaugeHP.tex";
    private const float NativeWidth = 96.0f;
    private const float NativeStripHeight = 16.0f;
    private const float FillOriginX = 5.0f;
    private const float BackdropOriginX = 6.0f;
    private const float TrackCapWidth = 10.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const DrawFlags TransitionDrawFlags = (DrawFlags)0xA;
    private const NodeFlags FillNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;
    private const NodeFlags BackdropNodeFlags = FillNodeFlags | NodeFlags.AnchorRight;
    private const NodeFlags TransitionNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    public ProgressBarPartyListHpNode() : this(false) { }

    internal ProgressBarPartyListHpNode(bool preserveEnds)
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.Multiply;

        var parts = CreateParts();

        BackdropImageNode = CreateStockImage(12, parts, new Vector2(NativeWidth, NativeStripHeight),
            preserveEnds ? BackdropNodeFlags & ~NodeFlags.Visible : BackdropNodeFlags,
            GaugeDrawFlags, 0, origin: new Vector2(BackdropOriginX, 0.0f));

        MainFillNode = CreateFill(parts);

        IncreaseFillNode = CreateTransitionNode(9, 108, parts);
        DecreaseFillNode = CreateTransitionNode(10, 252, parts);
        IncreaseFillNode.Node->PartsList->Id = 1;
        DecreaseFillNode.Node->PartsList->Id = 1;

        if (preserveEnds)
            StaticBackdropNode = CreateSlicedLayer(13, parts[0], Vector2.Zero, TrackCapWidth,
                GaugeDrawFlags, FillNodeFlags);

        InitializeGauge(new Vector2(NativeWidth, NativeStripHeight), minimumUnscaledWidth: preserveEnds ? TrackCapWidth * 2 : 0);
    }

    private static NineGridNode CreateTransitionNode(uint nodeId, byte partsRenderType, Part[] parts)
        => new()
        {
            Parts = parts,
            NodeId = nodeId,
            NodeFlags = TransitionNodeFlags,
            DrawFlags = TransitionDrawFlags,
            PartId = 1,
            Position = new Vector2(0.0f, -NativeStripHeight),
            Size = new Vector2(0.0f, NativeStripHeight * 3.0f),
            Origin = new Vector2(FillOriginX, 0.0f),
            LeftOffset = 6,
            RightOffset = 6,
            BlendMode = 2,
            PartsRenderType = partsRenderType,
        };

    private static NineGridNode CreateFill(Part[] parts)
    {
        var node = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = FillNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 2,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeStripHeight),
            Origin = new Vector2(FillOriginX, 0.0f),
            LeftOffset = 6,
            RightOffset = 6,
            PartsRenderType = 240,
            Parts = parts,
        };
        node.Node->PartsList->Id = 1;
        return node;
    }

    private static Part[] CreateParts()
        => [
            new() { Id = 0, TexturePath = TexturePath, TextureCoordinates = new Vector2(16.0f, 0.0f), Size = new Vector2(96.0f, 16.0f) },
            new() { Id = 1, TexturePath = TexturePath, TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(16.0f, 48.0f) },
            new() { Id = 2, TexturePath = TexturePath, TextureCoordinates = new Vector2(16.0f, 16.0f), Size = new Vector2(16.0f, 16.0f) },
            new() { Id = 3, TexturePath = TexturePath, TextureCoordinates = new Vector2(64.0f, 16.0f), Size = new Vector2(48.0f, 16.0f) },
            new() { Id = 4, TexturePath = TexturePath, TextureCoordinates = new Vector2(32.0f, 16.0f), Size = new Vector2(16.0f, 16.0f) },
            new() { Id = 5, TexturePath = TexturePath, TextureCoordinates = new Vector2(16.0f, 32.0f), Size = new Vector2(16.0f, 16.0f) },
            new() { Id = 6, TexturePath = TexturePath, TextureCoordinates = new Vector2(112.0f, 0.0f), Size = new Vector2(16.0f, 48.0f) },
            new() { Id = 7, TexturePath = TexturePath, TextureCoordinates = new Vector2(48.0f, 16.0f), Size = new Vector2(16.0f, 16.0f) },
            new() { Id = 8, TexturePath = TexturePath, TextureCoordinates = new Vector2(32.0f, 32.0f), Size = new Vector2(16.0f, 16.0f) },
        ];
}
