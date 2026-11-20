using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.Components.ProgressBars;

public abstract unsafe class GaugeBarNode : ComponentNode<AtkComponentGaugeBar, AtkUldComponentDataGaugeBar>
{
    private const int GaugeMaximum = 10000;
    private readonly List<NodeLayout> layouts = [];
    private Vector2 nativeSize;
    private Vector2 requestedSize;
    private float gaugeWidth;
    private float layoutWidth;
    private float minimumUnscaledWidth;
    private float progress;
    private bool fillRightToLeft;
    private bool isInitialized;
    private Vector4? barColor;

    private float GaugeLayoutWidth => GaugeGeometry.GetInnerWidth(layoutWidth, nativeSize.X, gaugeWidth);

    private sealed record NodeLayout(NodeBase Node, Vector2 Position, Vector2 Size, Vector3 MultiplyColor);

    public ImageNode BackdropImageNode { get; protected set; } = null!;
    protected NineGridNode? FrameUnderlayNode { get; set; }
    public NineGridNode? StaticBackdropNode { get; protected set; }
    public NineGridNode MainFillNode { get; protected set; } = null!;
    public NineGridNode? IncreaseFillNode { get; protected set; }
    public NineGridNode? DecreaseFillNode { get; protected set; }
    public NineGridNode? BorderNode { get; protected set; }
    public ImageNode? BorderImageNode { get; protected set; }

    protected GaugeColorMode BackgroundColorMode { get; set; } = GaugeColorMode.Multiply;
    protected virtual GaugeColorMode BarColorMode => GaugeColorMode.Multiply;
    protected virtual Vector3? BarMultiplyColor => null;

    public override Vector2 Size
    {
        get => requestedSize;
        set
        {
            requestedSize = new Vector2(Math.Max(1, value.X), Math.Max(1, value.Y));
            if (!isInitialized) return;
            var preserveEnds = minimumUnscaledWidth > 0;
            layoutWidth = preserveEnds
                ? GaugeGeometry.GetUnscaledWidth(requestedSize, nativeSize.Y, minimumUnscaledWidth)
                : nativeSize.X;
            var layoutHeight = preserveEnds ? nativeSize.Y : requestedSize.Y;
            base.Size = new Vector2(GaugeLayoutWidth, layoutHeight);
            ScaleX = requestedSize.X / layoutWidth;
            ScaleY = preserveEnds ? requestedSize.Y / layoutHeight : 1;
        }
    }

    public override float Width
    {
        get => requestedSize.X;
        set => Size = requestedSize with { X = value };
    }

    public override float Height
    {
        get => requestedSize.Y;
        set => Size = requestedSize with { Y = value };
    }

    public float Progress
    {
        get => progress;
        set
        {
            var normalized = float.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0;
            if (progress == normalized) return;
            progress = normalized;
            UpdateGaugeValue();
        }
    }

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value) return;
            fillRightToLeft = value;
            UpdateGaugeValue();
        }
    }

    public Vector4 BackgroundColor
    {
        get => GaugeColors.Get(StaticBackdropNode ?? (NodeBase)BackdropImageNode, BackgroundColorMode);
        set
        {
            var node = StaticBackdropNode ?? (NodeBase)BackdropImageNode;
            GaugeColors.Apply(node, value, BackgroundColorMode, GetLayout(node).MultiplyColor);
        }
    }

    public Vector4 BarColor
    {
        get => barColor ?? GaugeColors.Get(MainFillNode, BarColorMode);
        set
        {
            if (barColor == value) return;
            barColor = value;
            RefreshBarColor();
        }
    }

    protected GaugeBarNode()
    {
        SetInternalComponentType(ComponentType.GaugeBar);
        CollisionNode.NodeFlags = 0;
    }

    protected static ImageNode CreateStockImage(
        uint nodeId, Part[] parts, Vector2 size, NodeFlags flags, DrawFlags drawFlags, ushort partId,
        WrapMode wrapMode = WrapMode.Stretch, Vector2 origin = default)
    {
        var image = new ImageNode
        {
            NodeId = nodeId,
            NodeFlags = flags,
            DrawFlags = drawFlags,
            PartId = partId,
            Position = Vector2.Zero,
            Size = size,
            Origin = origin,
            WrapMode = wrapMode,
            ImageNodeFlags = 0,
        };
        image.AddPart(parts);
        image.Node->PartsList->Id = 1;
        return image;
    }

    protected static NineGridNode CreateSlicedLayer(uint nodeId, Part part, Vector2 position, float capWidth,
        DrawFlags drawFlags, NodeFlags nodeFlags)
        => new()
        {
            NodeId = nodeId,
            NodeFlags = nodeFlags,
            DrawFlags = drawFlags,
            Position = position,
            Size = part.Size,
            LeftOffset = capWidth,
            RightOffset = capWidth,
            Parts = [part],
        };

    protected void InitializeGauge(Vector2 size, float? internalWidth = null, bool useBorderSlot = false,
        float minimumUnscaledWidth = 0)
    {
        nativeSize = requestedSize = size;
        gaugeWidth = internalWidth ?? size.X;
        this.minimumUnscaledWidth = minimumUnscaledWidth;

        Attach(BackdropImageNode);
        Attach(FrameUnderlayNode);
        Attach(StaticBackdropNode);
        Attach(IncreaseFillNode);
        Attach(DecreaseFillNode);
        Attach(MainFillNode);
        Attach(BorderNode);
        Attach(BorderImageNode);

        Data->Nodes[0] = MainFillNode.NodeId;
        Data->Nodes[1] = BackdropImageNode.NodeId;
        Data->Nodes[2] = 0;
        Data->Nodes[3] = IncreaseFillNode?.NodeId ?? 0;
        Data->Nodes[4] = DecreaseFillNode?.NodeId ?? 0;
        Data->Nodes[5] = useBorderSlot ? BorderNode?.NodeId ?? BorderImageNode?.NodeId ?? 0 : 0;
        Data->MarginV = 0;
        Data->MarginH = 0;
        Data->Vertical = 0;
        Data->Indicator = 0;
        Data->Min = 0;
        Data->Max = GaugeMaximum;
        Data->Value = 0;

        InitializeComponentEvents();
        isInitialized = true;
        Size = requestedSize;
        UpdateGaugeValue();
    }

    private void Attach(NodeBase? node)
    {
        if (node == null) return;
        layouts.Add(new NodeLayout(node, node.Position, node.Size, node.MultiplyColor));
        node.Position = Vector2.Zero;
        node.Size = Vector2.Zero;
        node.AttachNode(this);
    }

    private NodeLayout GetLayout(NodeBase node)
        => layouts.Find(layout => ReferenceEquals(layout.Node, node))!;

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (!isInitialized) return;

        var preserveEnds = minimumUnscaledWidth > 0;
        var heightScale = preserveEnds ? 1 : requestedSize.Y / nativeSize.Y;
        if (preserveEnds)
        {
            CollisionNode.Size = new Vector2(GaugeLayoutWidth, nativeSize.Y);
            ComponentBase->UldManager.RootNodeWidth = (ushort)GaugeLayoutWidth;
            ComponentBase->UldManager.RootNodeHeight = (ushort)nativeSize.Y;
        }
        foreach (var layout in layouts)
        {
            layout.Node.Position = layout.Position * new Vector2(1, heightScale);
            if (layout.Node == MainFillNode || layout.Node == IncreaseFillNode || layout.Node == DecreaseFillNode)
                layout.Node.Height = layout.Size.Y * heightScale;
            else
            {
                var width = preserveEnds ? GaugeGeometry.GetInnerWidth(layoutWidth, nativeSize.X, layout.Size.X) : layout.Size.X;
                layout.Node.Size = new Vector2(width, layout.Size.Y * heightScale);
            }
        }

        ComponentBase->Setup();
        UpdateGaugeValue();
    }

    private void UpdateGaugeValue()
    {
        if (IsDisposed || !isInitialized) return;

        var value = (int)Math.Round(progress * GaugeMaximum);
        Component->SetGaugeValue(value, value, true);
        ComponentBase->Update(0);

        var fill = GetLayout(MainFillNode);
        var fullWidth = minimumUnscaledWidth > 0 ? GaugeLayoutWidth : fill.Size.X;
        MainFillNode.X = fill.Position.X + (fillRightToLeft ? Math.Max(0, fullWidth - MainFillNode.Width) : 0);
        var transitionX = fillRightToLeft ? MainFillNode.X : Math.Max(0, MainFillNode.Width - Component->MarginX);

        if (IncreaseFillNode != null)
        {
            IncreaseFillNode.X = transitionX;
            IncreaseFillNode.Width = 0;
        }
        if (DecreaseFillNode != null)
        {
            DecreaseFillNode.X = transitionX;
            DecreaseFillNode.Width = 0;
        }
    }

    protected void RefreshBarColor()
    {
        if (!isInitialized || !barColor.HasValue) return;
        ApplyFillColor(MainFillNode, barColor.Value);
        if (IncreaseFillNode != null) ApplyFillColor(IncreaseFillNode, barColor.Value);
        if (DecreaseFillNode != null) ApplyFillColor(DecreaseFillNode, barColor.Value);
    }

    private void ApplyFillColor(NodeBase node, Vector4 color)
        => GaugeColors.Apply(node, color, BarColorMode, BarMultiplyColor ?? GetLayout(node).MultiplyColor);
}
