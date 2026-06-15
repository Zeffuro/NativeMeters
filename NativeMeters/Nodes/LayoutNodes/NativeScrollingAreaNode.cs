using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.LayoutNodes;

public unsafe class NativeScrollingAreaNode<T> : ResNode where T : NodeBase, new()
{
    public CollisionNode ScrollingCollisionNode { get; }
    public ResNode ContentAreaClipNode { get; }
    public T ContentNode { get; }
    public ScrollBarNode ScrollBarNode { get; }

    public NativeScrollingAreaNode()
    {
        ContentAreaClipNode = new ResNode
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Clip | NodeFlags.EmitsEvents,
        };
        ContentAreaClipNode.AttachNode(this);

        ScrollingCollisionNode = new CollisionNode
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.HasCollision | NodeFlags.RespondToMouse | NodeFlags.EmitsEvents,
        };
        ScrollingCollisionNode.AttachNode(ContentAreaClipNode);

        ContentNode = new T
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents,
        };
        ContentNode.AttachNode(ContentAreaClipNode);

        ScrollBarNode = new ScrollBarNode
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft | NodeFlags.AnchorBottom |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.RespondToMouse | NodeFlags.EmitsEvents,
            IsAcceptingMouseWheelEvents = true,
        };
        ScrollBarNode.AttachNode(this);

        ((AtkResNode*)ContentAreaClipNode)->AtkEventManager.RegisterEvent(
            AtkEventType.MouseWheel,
            5,
            null,
            ScrollingCollisionNode,
            ScrollBarNode,
            false);

        ((AtkResNode*)ScrollingCollisionNode)->AtkEventManager.RegisterEvent(
            AtkEventType.MouseWheel,
            5,
            null,
            ScrollingCollisionNode,
            ScrollBarNode,
            false);

        ((AtkResNode*)ContentNode)->AtkEventManager.RegisterEvent(
            AtkEventType.MouseWheel,
            5,
            null,
            ScrollingCollisionNode,
            ScrollBarNode,
            false);
    }

    public int ScrollPosition
    {
        get => ScrollBarNode.ScrollPosition;
        set => ScrollBarNode.ScrollPosition = value;
    }

    public int ScrollSpeed
    {
        get => ScrollBarNode.ScrollSpeed;
        set => ScrollBarNode.ScrollSpeed = value;
    }

    public float ContentHeight
    {
        get => ContentNode.Height;
        set
        {
            ContentNode.Height = Math.Max(1.0f, value);
            ScrollBarNode.UpdateScrollParams();
        }
    }

    public bool AutoHideScrollBar
    {
        get => ScrollBarNode.HideWhenDisabled;
        set => ScrollBarNode.HideWhenDisabled = value;
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        var contentWidth = Math.Max(0.0f, Width - 16.0f);
        if (ContentNode.Height <= 0.0f)
        {
            ContentNode.Height = Math.Max(1.0f, Height);
        }

        ContentNode.Width = contentWidth;
        ContentAreaClipNode.Size = new Vector2(contentWidth, Height);
        ScrollingCollisionNode.Size = new Vector2(contentWidth, Height);

        ScrollBarNode.Size = new Vector2(8.0f, Height);
        ScrollBarNode.Position = new Vector2(Math.Max(0.0f, Width - 8.0f), 0.0f);
        ScrollBarNode.SetContentNodes(ContentNode, ScrollingCollisionNode);
    }
}
