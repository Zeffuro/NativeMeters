using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

    public void UpdateLayout()
    {
        var textSize = TextNode.GetTextDrawSize(considerScale: true);

        float textWidth = textSize.X > 0 ? textSize.X : 10;
        float textHeight = textSize.Y > 0 ? textSize.Y : 10;

        var newSize = new Vector2(textWidth + Padding.X * 2, textHeight + Padding.Y * 2);

        Size = newSize;

        TextNode.Position = Padding;
        BackgroundNode.Size = newSize;
    }

    public ReadOnlySeString String
    {
        get => TextNode.String;
        set { if (TextNode.String.ToString() == value.ToString()) return; TextNode.String = value; UpdateLayout(); }
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
        get => TextNode.BackgroundColor;
        set => TextNode.BackgroundColor = value;
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
        set => TextNode.AlignmentType = value;
    }
    public Vector2 Padding { get; set; } = new(6, 2);

    protected override void OnSizeChanged() => BackgroundNode.Size = Size;
}
