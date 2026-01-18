using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.LayoutNodes;

public class FakeCategoryNode : VerticalListNode
{
    private readonly NineGridNode background;
    private readonly ImageNode arrow;
    private readonly TextNode label;
    private readonly CollisionNode collision;
    private readonly VerticalListNode content;
    private readonly SimpleComponentNode header;

    private bool isCollapsed = true;
    private float headerHeight = 28.0f;

    public Action? OnToggle;

    public VerticalListNode Content => content;

    public float HeaderHeight
    {
        get => headerHeight;
        set
        {
            headerHeight = value;
            header.Height = value;
            background.Height = value;
            collision.Height = value;
            arrow.Y = (value - arrow.Height) / 2.0f;
            label.Height = value;
            RecalculateLayout();
        }
    }

    public float NestingIndent
    {
        get;
        set
        {
            field = value;
            arrow.X = value + 4.0f;
            label.X = value + 23.0f;
            content.X = value + 10.0f;
        }
    }

    public FakeCategoryNode()
    {
        FitContents = true;
        ItemSpacing = 0.0f;

        header = new SimpleComponentNode
        {
            Size = new Vector2(Width, headerHeight)
        };

        background = new SimpleNineGridNode {
            TexturePath = "ui/uld/ListItemB.tex",
            TextureSize = new Vector2(48.0f, 28.0f),
            TextureCoordinates = new Vector2(0.0f, 24.0f),
            Size = new Vector2(Width, headerHeight),
            TopOffset = 10, LeftOffset = 12, RightOffset = 12, BottomOffset = 12,
            Color = new Vector4(0.9f, 0.9f, 0.9f, 1.0f)
        };
        background.AttachNode(header);

        arrow = new ImageNode {
            Position = new Vector2(4.0f, 2.0f),
            Size = new Vector2(24.0f, 24.0f),
        };
        arrow.AddPart(
            new Part
            {
                TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(0, 0), Size = new Vector2(24, 24), Id = 0
            },
            new Part
            {
                TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(24, 0), Size = new Vector2(24, 24), Id = 1
            });
        arrow.AttachNode(header);

        label = new TextNode {
            Position = new Vector2(23.0f, 0.0f),
            Size = new Vector2(Width - 23, headerHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        label.AttachNode(header);

        collision = new CollisionNode
        {
            Size = new Vector2(Width, headerHeight),
            ShowClickableCursor = true
        };
        collision.AddEvent(AtkEventType.MouseClick, () => {
            isCollapsed = !isCollapsed;
            UpdateState();
            OnToggle?.Invoke();
        });
        collision.AttachNode(header);

        content = new VerticalListNode {
            FitContents = true,
            ItemSpacing = 4.0f,
            IsVisible = false,
        };

        AddNode([header, content]);

        UpdateState();
    }

    private void UpdateState()
    {
        content.IsVisible = !isCollapsed;
        arrow.PartId = isCollapsed ? 0u : 1u;

        content.RecalculateLayout();
        RecalculateLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (background == null || label == null || collision == null) return;

        header.Width = Width;
        background.Width = Width;
        label.Width = Math.Max(0, Width - label.X);
        collision.Width = Width;
        content.Width = Math.Max(0, Width - content.X);
    }

    public ReadOnlySeString String {
        get => label.String;
        set => label.String = value;
    }
}