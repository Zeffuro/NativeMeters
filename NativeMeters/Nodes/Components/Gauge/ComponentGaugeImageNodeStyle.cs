using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;

namespace NativeMeters.Nodes.Components.Gauge;

public sealed class ComponentGaugeImageNodeStyle
{
    public uint NodeId { get; init; }
    public uint PartId { get; init; }
    public Vector2 Position { get; init; }
    public Vector2 Size { get; init; }
    public Vector2 Origin { get; init; }
    public NodeFlags NodeFlags { get; init; }
    public DrawFlags DrawFlags { get; init; }
    public WrapMode WrapMode { get; init; } = WrapMode.Tile;
    public ImageNodeFlags ImageFlags { get; init; }
    public Vector4 Color { get; init; } = Vector4.One;
    public Vector3 MultiplyColor { get; init; } = Vector3.One;
    public Vector3 AddColor { get; init; } = Vector3.Zero;
}
