using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarLimitBreakGaugeNode() : ComponentGaugeProgressNode(Style)
{
    private const string TexturePath = "ui/uld/LimitBreak.tex";
    private const float NativeWidth = 164.0f;
    private const float NativeHeight = 20.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    private static ComponentGaugeProgressStyle Style { get; } = new()
    {
        PartsListId = 1,
        NativeWidth = NativeWidth,
        NativeHeight = NativeHeight,
        BackgroundColorMode = ComponentGaugeProgressColorMode.TextureAlpha,
        BarColorMode = ComponentGaugeProgressColorMode.BrightAdditive,
        NativeBarColorMode = ComponentGaugeProgressColorMode.BrightAdditive,
        Parts = CreateParts(),
        Backdrop = new ComponentGaugeImageNodeStyle
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageFlags = 0,
        },
        StaticBackdrop = new ComponentGaugeNineGridNodeStyle
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 1,
            Position = new Vector2(0.0f, 3.0f),
            Size = new Vector2(NativeWidth, 12.0f),
            Origin = Vector2.Zero,
        },
        MainFill = new ComponentGaugeNineGridNodeStyle
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
        },
        BorderImage = new ComponentGaugeImageNodeStyle
        {
            NodeId = 14,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageFlags = 0,
            Color = new Vector4(1.0f, 1.0f, 1.0f, 0.32f),
        },
    };

    private static IReadOnlyList<Part> CreateParts()
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
