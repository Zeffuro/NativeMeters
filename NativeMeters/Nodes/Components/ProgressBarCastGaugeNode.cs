using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarCastGaugeNode() : ComponentGaugeProgressNode(Style)
{
    private const string TexturePath = "ui/uld/Parameter_Gauge.tex";
    private const float NativeWidth = 160.0f;
    private const float NativeHeight = 20.0f;
    private static readonly Vector3 NativeCastFillMultiplyColor = new(0.9f, 0.75f, 0.75f);
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    private static ComponentGaugeProgressStyle Style { get; } = new()
    {
        PartsListId = 1,
        NativeWidth = NativeWidth,
        NativeHeight = NativeHeight,
        BackgroundColorMode = ComponentGaugeProgressColorMode.TextureAlpha,
        BarColorMode = ComponentGaugeProgressColorMode.Additive,
        Parts = CreateParts(),
        Backdrop = new ComponentGaugeImageNodeStyle
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 5,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageFlags = 0,
        },
        MainFill = new ComponentGaugeNineGridNodeStyle
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
        },
        BorderImage = new ComponentGaugeImageNodeStyle
        {
            NodeId = 13,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Stretch,
            ImageFlags = 0,
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
