using System;

namespace NativeMeters.Rendering;

public static class ViewUtils
{
    public static float CalculateProgressRatio(double value, double max)
    {
        if (max <= 0.0)
            return 0.06f;
        float ratio = (float)Math.Clamp(value / max, 0.0, 1.0);
        return 0.06f + (1f - 0.06f) * ratio;
    }
}
