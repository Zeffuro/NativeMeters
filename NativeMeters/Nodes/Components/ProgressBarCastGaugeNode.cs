using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarCastGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/Parameter_Gauge.tex";
    private const float NativeWidth = 160.0f;
    private const float NativeHeight = 20.0f;
    private static readonly Vector3 NativeCastFillMultiplyColor = new(0.9f, 0.75f, 0.75f);
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    public ProgressBarCastGaugeNode()
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.Additive;

        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 5,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageNodeFlags = 0,
        };
        BackdropImageNode.AddPart(CreateParts());
        BackdropImageNode.Node->PartsList->Id = 1;

        MainFillNode = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 2,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = new Vector2(6.0f, 0.0f),
            LeftOffset = 7.0f,
            RightOffset = 7.0f,
            MultiplyColor = NativeCastFillMultiplyColor,
            PartsRenderType = 36,
            Parts = CreateParts(),
        };
        MainFillNode.Node->PartsList->Id = 1;

        BorderImageNode = new ImageNode
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageNodeFlags = 0,
        };
        BorderImageNode.AddPart(CreateParts());
        BorderImageNode.Node->PartsList->Id = 1;

        InitializeGauge(new Vector2(NativeWidth, NativeHeight));
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
