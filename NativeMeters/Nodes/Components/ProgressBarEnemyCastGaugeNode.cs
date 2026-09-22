using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public unsafe class ProgressBarEnemyCastGaugeNode : ComponentGaugeProgressNode
{
    private const string TexturePath = "ui/uld/PartyList_GaugeCast.tex";
    private const float NativeWidth = 204.0f;
    private const float GaugeWidth = 188.0f;
    private const float NativeHeight = 20.0f;
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags VisibleGaugeNodeFlags =
        NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;

    public ProgressBarEnemyCastGaugeNode()
    {
        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.Multiply;

        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = VisibleGaugeNodeFlags,
            DrawFlags = GaugeDrawFlags,
            PartId = 1,
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
            PartId = 0,
            Position = new Vector2(8.0f, 7.0f),
            Size = new Vector2(GaugeWidth, 7.0f),
            Origin = Vector2.Zero,
            LeftOffset = 10.0f,
            RightOffset = 10.0f,
            PartsRenderType = 240,
            Parts = CreateParts(),
        };
        MainFillNode.Node->PartsList->Id = 1;

        InitializeGauge(new Vector2(NativeWidth, NativeHeight), GaugeWidth);
    }

    private static Part[] CreateParts()
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
