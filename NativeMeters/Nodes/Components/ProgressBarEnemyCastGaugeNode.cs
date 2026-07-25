using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarEnemyCastGaugeNode() : ComponentGaugeProgressNode(Style)
{
    private const string TexturePath = "ui/uld/PartyList_GaugeCast.tex";
    private const float NativeWidth = 204.0f;
    private const float GaugeWidth = 188.0f;
    private const float NativeHeight = 20.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    private static ComponentGaugeProgressStyle Style { get; } = new()
    {
        PartsListId = 1,
        NativeWidth = NativeWidth,
        NativeHeight = NativeHeight,
        GaugeNativeWidth = GaugeWidth,
        BackgroundColorMode = ComponentGaugeProgressColorMode.TextureAlpha,
        BarColorMode = ComponentGaugeProgressColorMode.Multiply,
        Parts = CreateParts(),
        Backdrop = new ComponentGaugeImageNodeStyle
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 1,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeHeight),
            Origin = Vector2.Zero,
            WrapMode = WrapMode.Tile,
            ImageFlags = 0,
        },
        MainFill = new ComponentGaugeNineGridNodeStyle
        {
            NodeId = 11,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = new Vector2(8.0f, 7.0f),
            Size = new Vector2(GaugeWidth, 7.0f),
            Origin = Vector2.Zero,
            LeftOffset = 10.0f,
            RightOffset = 10.0f,
            PartsRenderType = 240,
        },
    };

    private static IReadOnlyList<Part> CreateParts()
        => [
            new()
            {
                Id = 0,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(8.0f, 3.0f),
                Size = new Vector2(GaugeWidth, 7.0f),
            },
            new()
            {
                Id = 1,
                TexturePath = TexturePath,
                TextureCoordinates = new Vector2(0.0f, 12.0f),
                Size = new Vector2(NativeWidth, NativeHeight),
            },
        ];
}
