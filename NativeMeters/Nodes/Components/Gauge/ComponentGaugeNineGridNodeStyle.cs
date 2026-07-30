using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;

namespace NativeMeters.Nodes.Components.Gauge;

public sealed class ComponentGaugeNineGridNodeStyle
{
    public uint NodeId { get; init; }
    public uint PartId { get; init; }
    public Vector2 Position { get; init; }
    public Vector2 Size { get; init; }
    public Vector2 Origin { get; init; }
    public NodeFlags NodeFlags { get; init; }
    public DrawFlags DrawFlags { get; init; }
    public float TopOffset { get; init; }
    public float BottomOffset { get; init; }
    public float LeftOffset { get; init; }
    public float RightOffset { get; init; }
    public uint BlendMode { get; init; }
    public byte PartsRenderType { get; init; }
    public Vector4 Color { get; init; } = Vector4.One;
    public Vector3 MultiplyColor { get; init; } = Vector3.One;
    public Vector3 AddColor { get; init; } = Vector3.Zero;
}
