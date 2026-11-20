using System;
using System.Numerics;

namespace NativeMeters.Rendering;

internal static class GaugeGeometry
{
    public const float CastWidth = 160f;
    public const float CastHeight = 20f;
    public const float CastFillInset = 6f;
    public const float CastTrackCapWidth = 16f;

    public static float GetUnscaledWidth(Vector2 size, float nativeHeight, float minimumWidth)
        => Math.Clamp(MathF.Round(size.X * nativeHeight / size.Y), minimumWidth, ushort.MaxValue);

    public static float GetInnerWidth(float layoutWidth, float nativeWidth, float nativeGaugeWidth)
        => layoutWidth - (nativeWidth - nativeGaugeWidth);
}
