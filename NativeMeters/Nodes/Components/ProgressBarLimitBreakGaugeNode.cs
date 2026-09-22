using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarLimitBreakGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/LimitBreak.tex";
    private const float NativeWidth = 164.0f;
    private const float NativeHeight = 20.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    public ProgressBarLimitBreakGaugeNode()
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.BrightAdditive;
        NativeColorMode = GaugeColorMode.BrightAdditive;

        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageNodeFlags = 0,
        };
        BackdropImageNode.AddPart(CreateParts());
        BackdropImageNode.Node->PartsList->Id = 1;

        StaticBackdropNode = new NineGridNode
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 1,
            Position = new Vector2(0.0f, 3.0f),
            Size = new Vector2(NativeWidth, 12.0f),
            Origin = Vector2.Zero,
            Parts = CreateParts(),
        };
        StaticBackdropNode.Node->PartsList->Id = 1;

        MainFillNode = new NineGridNode
        {
            NodeId = 11,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 2,
            Position = new Vector2(0.0f, 1.0f),
            Size = new Vector2(NativeWidth, 16.0f),
            Origin = new Vector2(18.0f, 0.0f),
            LeftOffset = 18.0f,
            RightOffset = 18.0f,
            PartsRenderType = 240,
            Parts = CreateParts(),
        };
        MainFillNode.Node->PartsList->Id = 1;

        BorderImageNode = new ImageNode
        {
            NodeId = 14,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageNodeFlags = 0,
            Color = new Vector4(1.0f, 1.0f, 1.0f, 0.32f),
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
                Size = new Vector2(NativeWidth, 12.0f),
            },
            new()
            {
                Id = 2,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 32.0f),
                Size = new Vector2(NativeWidth, 16.0f),
            },
        ];
}
