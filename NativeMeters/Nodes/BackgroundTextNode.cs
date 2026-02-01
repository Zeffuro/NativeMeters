using System;
using System.Drawing;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes;

public class BackgroundTextNode : SimpleComponentNode
{
    public readonly NineGridNode BackgroundNode;
    public readonly TextNode TextNode;

    public BackgroundTextNode()
    {
        BackgroundNode = new SimpleNineGridNode {
            TexturePath = "ui/uld/ToolTipS.tex",
            TextureCoordinates = Vector2.Zero,
            TextureSize = new Vector2(32.0f, 24.0f),
            TopOffset = 10, BottomOffset = 10, LeftOffset = 15, RightOffset = 15,
            Position = Vector2.Zero,
            AddColor = KnownColor.Black.Vector3()
        };
        BackgroundNode.AttachNode(this);

        TextNode = new TextNode {
            TextFlags = TextFlags.Edge,
            FontType = FontType.TrumpGothic,
            FontSize = 18,
            AlignmentType = AlignmentType.Left,
            Position = Vector2.Zero,
        };
        TextNode.AttachNode(this);
    }

    private ReadOnlySeString lastLayoutString = string.Empty;

    public void UpdateLayout()
    {
        var currentString = TextNode.String;

        if (currentString == lastLayoutString) return;
        lastLayoutString = currentString;

        RecalculateBackgroundSize();
    }

    private void RecalculateBackgroundSize()
    {
        var textSize = TextNode.GetTextDrawSize(considerScale: false);

        float textWidth = textSize.X > 0 ? textSize.X : 10.0f;
        float textHeight = textSize.Y > 0 ? textSize.Y : 10.0f;

        // Background size based on actual text size plus padding
        var bgSize = new Vector2(
            Math.Max(30.0f, textWidth + Padding.X * 2),
            Math.Max(20.0f, textHeight + Padding.Y * 2)
        );

        BackgroundNode.Size = bgSize;

        float bgX = TextNode.AlignmentType switch
        {
            AlignmentType.Center or AlignmentType.Top or AlignmentType.Bottom
                => (TextNode.Width - bgSize.X) / 2,

            AlignmentType.Right or AlignmentType.TopRight or AlignmentType.BottomRight
                => TextNode.Width - bgSize.X + Padding.X,

            _ => -Padding.X
        };

        float bgY = TextNode.AlignmentType switch
        {
            AlignmentType.Bottom or AlignmentType.BottomLeft or AlignmentType.BottomRight
                => TextNode.Height - bgSize.Y + Padding.Y,

            AlignmentType.Top or AlignmentType.TopLeft or AlignmentType.TopRight
                => -Padding.Y,

            _ => (TextNode.Height - bgSize.Y) / 2
        };

        BackgroundNode.Position = new Vector2(bgX, bgY);
    }

    public ReadOnlySeString String
    {
        get => TextNode.String;
        set
        {
            if (TextNode.String.ToString() == value.ToString()) return;
            TextNode.String = value;
            UpdateLayout();
        }
    }

    public int FontSize
    {
        get => (int)TextNode.FontSize;
        set { TextNode.FontSize = (uint)value; UpdateLayout(); }
    }

    public FontType FontType
    {
        get => TextNode.FontType;
        set { TextNode.FontType = value; UpdateLayout(); }
    }

    public bool ShowBackground
    {
        get => BackgroundNode.IsVisible;
        set => BackgroundNode.IsVisible = value;
    }

    public Vector4 BackgroundColor
    {
        get;
        set
        {
            field = value;
            BackgroundNode.Color = new Vector4(1, 1, 1, value.W);
            BackgroundNode.AddColor = new Vector3(value.X, value.Y, value.Z);
        }
    }

    public Vector4 TextColor
    {
        get => TextNode.TextColor;
        set => TextNode.TextColor = value;
    }

    public Vector4 TextOutlineColor
    {
        get => TextNode.TextOutlineColor;
        set => TextNode.TextOutlineColor = value;
    }

    public TextFlags TextFlags
    {
        get => TextNode.TextFlags;
        set
        {
            TextNode.TextFlags = value;
            UpdateLayout();
        }
    }

    public AlignmentType AlignmentType
    {
        get => TextNode.AlignmentType;
        set
        {
            TextNode.AlignmentType = value;
            RecalculateBackgroundSize();
        }
    }

    public Vector2 Padding { get; set; } = new(6, 2);

    protected override void OnSizeChanged()
    {
        TextNode.Size = Size;
        RecalculateBackgroundSize();
    }
}
