using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarPartyListHpNode() : ComponentGaugeProgressNode(Style)
{
    private const string TexturePath = "ui/uld/PartyList_GaugeHP.tex";
    private const float NativeWidth = 96.0f;
    private const float NativeStripHeight = 16.0f;
    private const float FillOriginX = 5.0f;
    private const float BackdropOriginX = 6.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const DrawFlags TransitionDrawFlags = (DrawFlags)0xA;
    private const NodeFlags FillNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;
    private const NodeFlags BackdropNodeFlags = FillNodeFlags | NodeFlags.AnchorRight;
    private const NodeFlags TransitionNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    private static ComponentGaugeProgressStyle Style { get; } = new()
    {
        PartsListId = 1,
        NativeWidth = NativeWidth,
        NativeHeight = NativeStripHeight,
        TransitionFillHeight = NativeStripHeight * 3.0f,
        BackgroundColorMode = ComponentGaugeProgressColorMode.TextureAlpha,
        BarColorMode = ComponentGaugeProgressColorMode.Multiply,
        Parts = CreateParts(),
        Backdrop = new ComponentGaugeImageNodeStyle
        {
            NodeId = 12,
            NodeFlags = BackdropNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 0,
            Position = Vector2.Zero,
            Size = new Vector2(NativeWidth, NativeStripHeight),
            Origin = new Vector2(BackdropOriginX, 0.0f),
            WrapMode = WrapMode.Stretch,
            ImageFlags = 0,
        },
        MainFill = new ComponentGaugeNineGridNodeStyle
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
        },
        IncreaseFill = CreateTransitionNodeStyle(9, 108),
        DecreaseFill = CreateTransitionNodeStyle(10, 252),
    };

    private static ComponentGaugeNineGridNodeStyle CreateTransitionNodeStyle(uint nodeId, byte partsRenderType)
        => new()
        {
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

    private static IReadOnlyList<Part> CreateParts()
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
