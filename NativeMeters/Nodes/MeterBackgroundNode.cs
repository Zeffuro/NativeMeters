using System;
using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes;

public sealed class MeterBackgroundNode : ResNode
{
    private readonly SimpleNineGridNode tooltipNode;
    private readonly WindowBackgroundTextureNode windowNode;
    private readonly WindowBackgroundTextureNode borderNode;
    private readonly SimpleImageNode gradientNode;

    public MeterBackgroundNode()
    {
        tooltipNode = new SimpleNineGridNode
        {
            TexturePath = "ui/uld/ToolTipS.tex",
            TextureCoordinates = Vector2.Zero,
            TextureSize = new Vector2(32.0f, 24.0f),
            Offsets = new Vector4(10, 10, 15, 15),
        };
        tooltipNode.AttachNode(this);

        windowNode = new WindowBackgroundTextureNode(false)
        {
            Offsets = new Vector4(64, 32, 32, 32),
            PartsRenderType = 19,
            IsVisible = false,
        };
        windowNode.AttachNode(this);

        borderNode = new WindowBackgroundTextureNode(true)
        {
            Offsets = new Vector4(64, 32, 32, 32),
            PartsRenderType = 7,
            IsVisible = false,
        };
        borderNode.AttachNode(this);

        gradientNode = new SimpleImageNode
        {
            TexturePath = "ui/uld/WindowA_Gradation.tex",
            TextureCoordinates = new Vector2(6, 2),
            TextureSize = new Vector2(24),
            WrapMode = WrapMode.Stretch,
            Position = new Vector2(4),
            IsVisible = false,
        };
        gradientNode.AttachNode(this);
    }

    public MeterBackgroundStyle Style
    {
        get;
        set
        {
            field = value;
            var isWindow = value is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused;
            tooltipNode.IsVisible = !isWindow;
            windowNode.IsVisible = isWindow;
            borderNode.IsVisible = value == MeterBackgroundStyle.WindowFocused;
            gradientNode.IsVisible = isWindow;
        }
    }

    public Vector4 BackgroundColor
    {
        get;
        set
        {
            field = value;
            tooltipNode.Color = windowNode.Color = borderNode.Color = gradientNode.Color = new Vector4(1, 1, 1, value.W);
            tooltipNode.AddColor = windowNode.AddColor = borderNode.AddColor = gradientNode.AddColor = new Vector3(value.X, value.Y, value.Z);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        tooltipNode.Size = Size;
        var nativeSize = Vector2.Max(Size, new Vector2(64, 96));
        windowNode.Size = borderNode.Size = nativeSize;
        windowNode.Scale = borderNode.Scale = Size / nativeSize;
        gradientNode.Size = new Vector2(Math.Max(0, Width - 8), Math.Max(0, Height - 16));
    }
}
