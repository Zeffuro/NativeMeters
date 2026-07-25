using System.Numerics;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components;

public interface IMeterProgressNode
{
    float Progress { get; set; }

    Vector4 BarColor { get; set; }

    Vector4 BackgroundColor { get; set; }

    ProgressBarColorTreatment ColorTreatment { get; set; }

    bool FillRightToLeft { get; set; }
}
