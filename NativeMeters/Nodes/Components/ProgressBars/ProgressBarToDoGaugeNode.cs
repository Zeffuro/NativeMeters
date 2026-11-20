using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Components.ProgressBars;

public unsafe class ProgressBarToDoGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/ToDoList.tex";
    private const float NativeWidth = 44.0f;
    private const float NativeHeight = 12.0f;
    private const float TrackCapWidth = 6.0f;
    private const DrawFlags GaugeDrawFlags = 0;
    private const NodeFlags HiddenGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled | NodeFlags.EmitsEvents;
    private const NodeFlags VisibleGaugeNodeFlags =
        HiddenGaugeNodeFlags | NodeFlags.Visible;

    public ProgressBarToDoGaugeNode() : this(false) { }

    internal ProgressBarToDoGaugeNode(bool preserveEnds)
    {
        BackgroundColorMode = GaugeColorMode.Flat;
        DefaultColorMode = GaugeColorMode.Flat;
        NativeColorMode = GaugeColorMode.Additive;

        var parts = CreateParts();

        BackdropImageNode = CreateStockImage(12, parts, new Vector2(NativeWidth, NativeHeight),
            HiddenGaugeNodeFlags, GaugeDrawFlags, 0, WrapMode.Tile);

        StaticBackdropNode = CreateTrack(parts);
        MainFillNode = CreateFill(parts);

        InitializeGauge(new Vector2(NativeWidth, NativeHeight), minimumUnscaledWidth: preserveEnds ? TrackCapWidth * 2 : 0);
    }

    private static NineGridNode CreateTrack(Part[] parts)
    {
        var node = new NineGridNode
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            LeftOffset = TrackCapWidth,
            RightOffset = TrackCapWidth,
            Parts = parts,
        };
        node.Node->PartsList->Id = 1;
        return node;
    }

    private static NineGridNode CreateFill(Part[] parts)
    {
        var node = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 1,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            LeftOffset = 4.0f,
            RightOffset = 4.0f,
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
                TextureCoordinates = new Vector2(108.0f, 8.0f),
                Size = new Vector2(44.0f, 12.0f),
            },
            new()
            {
                Id = 1,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(112.0f, 0.0f),
                Size = new Vector2(40.0f, 8.0f),
            },
        ];
}
