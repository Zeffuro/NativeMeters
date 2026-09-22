using System.Numerics;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public abstract class ComponentGaugeProgressNode : GaugeBarNode, IMeterProgressNode
{
    protected GaugeColorMode DefaultColorMode { get; set; } = GaugeColorMode.Multiply;
    protected GaugeColorMode? NativeColorMode { get; set; }

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
        ProgressBarColorTreatment.Flat => GaugeColorMode.Flat,
        ProgressBarColorTreatment.NativeTint => NativeColorMode ?? DefaultColorMode,
        ProgressBarColorTreatment.LegacyAdditive => GaugeColorMode.Additive,
        _ => DefaultColorMode,
    };

    protected override Vector3? BarMultiplyColor => ColorTreatment == ProgressBarColorTreatment.LegacyAdditive
        ? new Vector3(90.0f, 75.0f, 75.0f) / 255.0f
        : null;
}
