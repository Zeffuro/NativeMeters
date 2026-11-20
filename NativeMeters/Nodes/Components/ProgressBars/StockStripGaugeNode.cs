using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components.ProgressBars;

internal sealed class StockStripGaugeNode : ComponentGaugeProgressNode
{
    private const DrawFlags GaugeDrawFlags = (DrawFlags)0x8;
    private const NodeFlags HiddenFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.Enabled;
    private const NodeFlags VisibleFlags = HiddenFlags | NodeFlags.Visible;

    private readonly record struct Crop(int U, int V, int Width, int Height, int CapWidth)
    {
        public Part CreatePart(string texture)
            => new()
            {
                TexturePath = texture,
                TextureCoordinates = new Vector2(U, V),
                Size = new Vector2(Width, Height),
            };
    }

    private readonly record struct Strip(string Texture, int Width, Crop Track, Crop Fill, int FillX = 0,
        int FillOriginX = 0, Crop? Frame = null);

    private static readonly Strip Aether = new("ui/uld/AetherGauge.tex", 94,
        new(0, 0, 36, 20, 14), new(0, 21, 36, 10, 7), FillOriginX: 7);
    private static readonly Strip Deliveries = new("ui/uld/SatisfactionSupply.tex", 300,
        new(0, 444, 92, 20, 10), new(0, 464, 92, 20, 10));
    private static readonly Strip Inspection = new("ui/uld/Gacha.tex", 198,
        new(0, 239, 198, 11, 10), new(1, 251, 194, 9, 8), FillX: 2);
    private static readonly Strip Negotiation = new("ui/uld/Negotiation.tex", 390,
        new(0, 192, 68, 16, 10), new(0, 208, 68, 16, 10));
    private static readonly Strip GoldSaucer = new("ui/uld/MinerBotanist.tex", 350,
        new(124, 160, 56, 32, 17), new(68, 160, 56, 32, 23), Frame: new Crop(68, 128, 56, 32, 17));

    public StockStripGaugeNode(ProgressBarType type, bool preserveEnds)
    {
        var art = type switch
        {
            ProgressBarType.AetherGauge => Aether,
            ProgressBarType.CustomDeliveries => Deliveries,
            ProgressBarType.ItemInspection => Inspection,
            ProgressBarType.CollectableNegotiation => Negotiation,
            ProgressBarType.GoldSaucerGauge => GoldSaucer,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        BackgroundColorMode = GaugeColorMode.TextureAlpha;
        DefaultColorMode = GaugeColorMode.BrightAdditive;
        FlatColorMode = GaugeColorMode.Solid;

        var outerSize = new Vector2(art.Width, art.Track.Height);
        var innerWidth = art.Width - art.FillX * 2;
        var fillPosition = new Vector2(art.FillX, (art.Track.Height - art.Fill.Height) / 2f);
        var track = art.Track.CreatePart(art.Texture);

        BackdropImageNode = new ImageNode
        {
            NodeId = 12,
            NodeFlags = HiddenFlags,
            DrawFlags = GaugeDrawFlags,
            Size = outerSize,
            WrapMode = WrapMode.Stretch,
        };
        BackdropImageNode.AddPart([track]);

        var trackLayer = CreateSlicedLayer(13, track, Vector2.Zero, art.Track.CapWidth, GaugeDrawFlags, VisibleFlags);
        trackLayer.Size = outerSize;

        if (type == ProgressBarType.AetherGauge)
            BorderNode = trackLayer;
        else
            StaticBackdropNode = trackLayer;

        MainFillNode = CreateSlicedLayer(11, art.Fill.CreatePart(art.Texture), fillPosition, art.Fill.CapWidth,
            GaugeDrawFlags, VisibleFlags);
        MainFillNode.Size = new Vector2(innerWidth, art.Fill.Height);
        MainFillNode.Origin = new Vector2(art.FillOriginX, 0);

        if (art.Frame is { } frame)
        {
            BorderNode = CreateSlicedLayer(14, frame.CreatePart(art.Texture), Vector2.Zero, frame.CapWidth,
                GaugeDrawFlags, VisibleFlags);
            BorderNode.Size = outerSize;
        }

        var edgeWidth = Math.Max(art.Track.CapWidth, art.Frame?.CapWidth ?? 0);
        var minimumWidth = 2 * Math.Max(edgeWidth, art.FillX + art.Fill.CapWidth);
        InitializeGauge(outerSize, innerWidth, minimumUnscaledWidth: preserveEnds ? minimumWidth : 0);
    }
}
