using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;

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

    public string DataSource { get; set; } = "[name]";

    public uint FontSize { get; set; } = 14;
    public FontType FontType { get; set; } = FontType.Axis;
    public TextFlags TextFlags { get; set; } = TextFlags.Edge;

    public Vector4 Color { get; set; } = ColorHelper.GetColor(54);
    public bool UseJobColor { get; set; } = false;
    public bool ShowOutline { get; set; } = true;
    public Vector4 OutlineColor { get; set; } = ColorHelper.GetColor(53);

    public bool ShowBackground { get; set; }
}