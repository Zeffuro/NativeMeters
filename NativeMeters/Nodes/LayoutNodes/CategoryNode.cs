using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.LayoutNodes;

public class CategoryNode : VerticalListNode
{
    protected readonly NineGridNode BackgroundNode;
    protected readonly ImageNode ArrowNode;
    protected readonly TextNode LabelNode;
    protected new readonly CollisionNode CollisionNode;
    protected readonly TabbedVerticalListNode ContentNode;
    protected readonly SimpleComponentNode HeaderNode;

    private bool isCollapsed = true;
    private float headerHeight = 28.0f;

    public Action? OnToggle;

    public TabbedVerticalListNode CollapsibleContent => ContentNode;

    public bool IsCollapsed
    {
        get => isCollapsed;
        set { isCollapsed = value; UpdateState(); }
    }

    public float HeaderHeight
    {
        get => headerHeight;
        set
        {
            headerHeight = value;
            HeaderNode.Height = value;
            BackgroundNode.Height = value;
            CollisionNode.Height = value;
            ArrowNode.Y = (value - ArrowNode.Height) / 2.0f;
            LabelNode.Height = value;
            RecalculateLayout();
        }
    }

    public uint FontSize { get => LabelNode.FontSize; set => LabelNode.FontSize = value; }

    public float TabSize
    {
        get => ContentNode.TabSize;
        set => ContentNode.TabSize = value;
    }

    public int TabStep
    {
        get => ContentNode.TabStep;
        set => ContentNode.TabStep = value;
    }

    public bool FitChildWidth
    {
        get => ContentNode.FitWidth;
        set => ContentNode.FitWidth = value;
    }

    public float NestingIndent
    {
        get;
        set
        {
            field = value;
            ArrowNode.X = value + 4.0f;
            LabelNode.X = value + 23.0f;
            ContentNode.X = value + 10.0f;
        }
    }

    public CategoryNode()
    {
        FitContents = true;
        ItemSpacing = 0.0f;

        HeaderNode = new SimpleComponentNode
        {
            Size = new Vector2(Width, headerHeight)
        };

        BackgroundNode = new SimpleNineGridNode {
            TexturePath = "ui/uld/ListItemB.tex",
            TextureSize = new Vector2(48.0f, 28.0f),
            TextureCoordinates = new Vector2(0.0f, 24.0f),
            Size = new Vector2(Width, headerHeight),
            TopOffset = 10, LeftOffset = 12, RightOffset = 12, BottomOffset = 12,
            Color = new Vector4(0.9f, 0.9f, 0.9f, 1.0f)
        };
        BackgroundNode.AttachNode(HeaderNode);

        ArrowNode = new ImageNode { Position = new Vector2(4.0f, 2.0f), Size = new Vector2(24.0f, 24.0f) };
        ArrowNode.AddPart(
            new Part { TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(0, 0), Size = new Vector2(24, 24), Id = 0 },
            new Part { TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(24, 0), Size = new Vector2(24, 24), Id = 1 }
        );
        ArrowNode.AttachNode(HeaderNode);

        LabelNode = new TextNode {
            Position = new Vector2(30.0f, 0.0f),
            Size = new Vector2(Width - 23, headerHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        LabelNode.AttachNode(HeaderNode);

        CollisionNode = new CollisionNode
        {
            Size = new Vector2(Width, headerHeight),
            ShowClickableCursor = true
        };
        CollisionNode.AddEvent(AtkEventType.MouseClick, () => {
            IsCollapsed = !IsCollapsed;
            OnToggle?.Invoke();
        });
        CollisionNode.AttachNode(HeaderNode);

        ContentNode = new TabbedVerticalListNode {
            IsVisible = false,
            X = 18.0f,
            ItemVerticalSpacing = 4.0f,
            TabSize = 18.0f,
            FitWidth = true,
        };

        base.AddNode([HeaderNode, ContentNode]);
        UpdateState();
    }

    private void UpdateState()
    {
        ContentNode.IsVisible = !isCollapsed;
        ArrowNode.PartId = isCollapsed ? 0u : 1u;

        if (!isCollapsed)
        {
            ContentNode.Width = Math.Max(0, Width - ContentNode.X);
            ContentNode.RecalculateLayout();
        }

        RecalculateLayout();
        OnToggle?.Invoke();
    }

    public void AddTab(int tabAmount = 1) => ContentNode.AddTab(tabAmount);

    public void SubtractTab(int tabAmount = 1) => ContentNode.SubtractTab(tabAmount);

    public new void AddNode(NodeBase node) => ContentNode.AddNode(node);

    public new void AddNode(IEnumerable<NodeBase> nodes) => ContentNode.AddNode(nodes);

    public void AddNode(int tabIndex, NodeBase node) => ContentNode.AddNode(tabIndex, node);

    public void AddNode(int tabIndex, IEnumerable<NodeBase> nodes) => ContentNode.AddNode(tabIndex, nodes);

    public new void RemoveNode(NodeBase node) => ContentNode.RemoveNode(node);

    public new void Clear() => ContentNode.Clear();

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (BackgroundNode == null || LabelNode == null || CollisionNode == null) return;

        HeaderNode.Width = Width;
        BackgroundNode.Width = Width;
        LabelNode.Width = Math.Max(0, Width - LabelNode.X);
        CollisionNode.Width = Width;
        ContentNode.Width = Math.Max(0, Width - ContentNode.X);
    }

    public ReadOnlySeString String { get => LabelNode.String; set => LabelNode.String = value; }
}
