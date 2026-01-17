using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NativeMeters.Configuration;

public enum MeterComponentType { JobIcon, Text, ProgressBar, Background }

public class RowComponentSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MeterComponentType Type { get; set; }
    public string Name { get; set; } = "New Component";

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = new(100, 20);
    public int ZIndex { get; set; } = 0;

    public string DataSource { get; set; } = "Name";
    public uint FontSize { get; set; } = 14;
    public FontType FontType { get; set; } = FontType.Axis;
    public Vector4 Color { get; set; } = Vector4.One;
    public bool ShowBackground { get; set; }
}