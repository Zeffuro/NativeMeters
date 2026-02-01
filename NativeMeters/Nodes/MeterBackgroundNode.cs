using System.Numerics;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes;

public sealed class MeterBackgroundNode : SimpleNineGridNode
{
    public MeterBackgroundNode()
    {
        TexturePath = "ui/uld/ToolTipS.tex";
        TextureCoordinates = Vector2.Zero;
        TextureSize = new Vector2(32.0f, 24.0f);
        TopOffset = 10;
        BottomOffset = 10;
        LeftOffset = 15;
        RightOffset = 15;
        Position = Vector2.Zero;
    }

    public Vector4 BackgroundColor
    {
        get;
        set
        {
            field = value;
            Color = new Vector4(1, 1, 1, value.W);
            AddColor = new Vector3(value.X, value.Y, value.Z);
        }
    }
}
