using System.Collections.Generic;
using KamiToolKit.Classes;

namespace NativeMeters.Nodes.Components.Gauge;

public sealed class ComponentGaugeProgressStyle
{
    public required IReadOnlyList<Part> Parts { get; init; }
    public uint PartsListId { get; init; }
    public float? NativeWidth { get; init; }
    public float? NativeHeight { get; init; }
    public float? GaugeNativeWidth { get; init; }
    public float TransitionFillHeight { get; init; }
    public ComponentGaugeImageNodeStyle Backdrop { get; init; } = null!;
    public ComponentGaugeNineGridNodeStyle? StaticBackdrop { get; init; }
    public ComponentGaugeNineGridNodeStyle MainFill { get; init; } = null!;
    public ComponentGaugeNineGridNodeStyle? IncreaseFill { get; init; }
    public ComponentGaugeNineGridNodeStyle? DecreaseFill { get; init; }
    public ComponentGaugeNineGridNodeStyle? Border { get; init; }
    public ComponentGaugeImageNodeStyle? BorderImage { get; init; }
    public bool UseGaugeBorderSlot { get; init; }
    public ComponentGaugeProgressColorMode BackgroundColorMode { get; init; } = ComponentGaugeProgressColorMode.Multiply;
    public ComponentGaugeProgressColorMode BarColorMode { get; init; } = ComponentGaugeProgressColorMode.Multiply;
    public ComponentGaugeProgressColorMode? NativeBarColorMode { get; init; }
    public ComponentGaugeProgressFillDirection FillDirection { get; init; } = ComponentGaugeProgressFillDirection.LeftToRight;
}
