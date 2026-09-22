using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarToDoGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/ToDoList.tex";
    private const float NativeWidth = 44.0f;
    private const float NativeHeight = 12.0f;
    private const DrawFlags GaugeDrawFlags = 0;
    private const NodeFlags HiddenGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled | NodeFlags.EmitsEvents;
    private const NodeFlags VisibleGaugeNodeFlags =
        HiddenGaugeNodeFlags | NodeFlags.Visible;

    public ProgressBarToDoGaugeNode()
    {
        BackgroundColorMode = GaugeColorMode.Flat;
        DefaultColorMode = GaugeColorMode.Flat;
        NativeColorMode = GaugeColorMode.Additive;

        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = HiddenGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Tile,
            ImageNodeFlags = 0,
        };
        BackdropImageNode.AddPart(CreateParts());
        BackdropImageNode.Node->PartsList->Id = 1;

        StaticBackdropNode = new NineGridNode
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            LeftOffset = 6.0f,
            RightOffset = 6.0f,
            Parts = CreateParts(),
        };
        StaticBackdropNode.Node->PartsList->Id = 1;

        MainFillNode = new NineGridNode
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
            Parts = CreateParts(),
        };
        MainFillNode.Node->PartsList->Id = 1;

        InitializeGauge(new Vector2(NativeWidth, NativeHeight));
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
