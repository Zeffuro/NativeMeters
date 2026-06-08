using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.LayoutNodes;

public unsafe sealed class ManualScrollingNode<T> : ResNode where T : NodeBase, new()
{
    public ResNode ClippingContentNode { get; }
    public CollisionNode ScrollingCollisionNode { get; }
    public T ContentNode { get; }

    public int ScrollSpeed { get; set; } = 36;

    private int scrollPosition;

    public ManualScrollingNode()
    {
        ClippingContentNode = new ResNode
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Clip | NodeFlags.EmitsEvents,
        };
        ClippingContentNode.AttachNode(this);

        ScrollingCollisionNode = new CollisionNode
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.HasCollision | NodeFlags.RespondToMouse | NodeFlags.EmitsEvents,
        };
        ScrollingCollisionNode.AttachNode(ClippingContentNode);

        ContentNode = new T
        {
            NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorLeft |
                        NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents,
        };
        ContentNode.AttachNode(ClippingContentNode);

        ClippingContentNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);
        ScrollingCollisionNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);
        ContentNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);
    }

    public void ResetScroll()
    {
        scrollPosition = 0;
        ApplyScroll();
    }

    public void UpdateScrollParams()
    {
        scrollPosition = Math.Clamp(scrollPosition, 0, GetMaxScroll());
        ApplyScroll();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        ClippingContentNode.Size = Size;
        ScrollingCollisionNode.Size = Size;
        ContentNode.Width = Width;
        UpdateScrollParams();
    }

    private unsafe void OnMouseWheel(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData)
    {
        var maxScroll = GetMaxScroll();
        if (maxScroll <= 0) return;

        scrollPosition += atkEventData->IsScrollUp ? -ScrollSpeed : ScrollSpeed;
        scrollPosition = Math.Clamp(scrollPosition, 0, maxScroll);

        ApplyScroll();
        atkEvent->SetEventIsHandled();
    }

    private void ApplyScroll()
    {
        ContentNode.Y = -scrollPosition;
    }

    private int GetMaxScroll()
        => Math.Max(0, (int)MathF.Ceiling(ContentNode.Height - Height));
}
