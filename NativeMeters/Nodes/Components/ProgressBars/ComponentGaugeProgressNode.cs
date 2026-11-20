using System.Numerics;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components.ProgressBars;

public abstract class ComponentGaugeProgressNode : GaugeBarNode, IMeterProgressNode
{
    protected static readonly Vector3 LegacyAdditiveTextureWeight = new Vector3(90.0f, 75.0f, 75.0f) / 255.0f;
    protected GaugeColorMode DefaultColorMode { get; set; } = GaugeColorMode.Multiply;
    protected GaugeColorMode? NativeColorMode { get; set; }
    protected GaugeColorMode FlatColorMode { get; set; } = GaugeColorMode.Flat;

    public ProgressBarColorTreatment ColorTreatment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            RefreshBarColor();
        }
    }

    protected override GaugeColorMode BarColorMode => ColorTreatment switch
    {
        ProgressBarColorTreatment.Flat => FlatColorMode,
        ProgressBarColorTreatment.NativeTint => NativeColorMode ?? DefaultColorMode,
        ProgressBarColorTreatment.LegacyAdditive => GaugeColorMode.Additive,
        ProgressBarColorTreatment.StockColors => GaugeColorMode.TextureAlpha,
        _ => DefaultColorMode,
    };

    protected override Vector3? BarMultiplyColor => ColorTreatment == ProgressBarColorTreatment.LegacyAdditive
        ? LegacyAdditiveTextureWeight
        : null;
}
