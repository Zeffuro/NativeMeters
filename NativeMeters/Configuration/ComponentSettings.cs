using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Models;

namespace NativeMeters.Configuration;

public enum MeterComponentType { Text, JobIcon, ProgressBar, Background, MenuButton, Separator, Icon }
public enum ColorMode { Static, Job, Role }

public class ComponentSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MeterComponentType Type { get; set; }
    public string Name { get; set; } = "New Component";

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = new(100, 20);
    public int ZIndex { get; set; } = 0;

    public string DataSource { get; set; } = "[name]";

    public JobIconType JobIconType { get; set; } = JobIconType.Default;
    public uint IconId { get; set; } = 0;

    public uint FontSize { get; set; } = 14;
    public FontType FontType { get; set; } = FontType.Axis;
    public TextFlags TextFlags { get; set; } = TextFlags.Edge;
    public AlignmentType AlignmentType { get; set; } = AlignmentType.Left;
    public Vector4 TextColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 TextOutlineColor { get; set; } = ColorHelper.GetColor(51);
    public Vector4 TextBackgroundColor { get; set; } = KnownColor.Black.Vector();
    public Vector4 BarColor { get; set; } = ColorHelper.GetColor(50);
    public Vector4 BarBackgroundColor { get; set; } = KnownColor.Black.Vector();
    public ColorMode ColorMode { get; set; } = ColorMode.Job;
    public bool ShowBackground { get; set; }

    public ComponentSettings DeepCopy()
    {
        var clone = (ComponentSettings) MemberwiseClone();

        clone.Id = Guid.NewGuid().ToString();
        clone.Name = Name + " (Copy)";
        clone.Position = Position + new Vector2(10, 10);

        return clone;
    }
}
