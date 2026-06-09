using System;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Extensions;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.LayoutNodes;

internal static class LayoutRecalculation
{
    public static void RecalculateBottomUp(NodeBase node)
    {
        if (!node.IsVisible) return;

        if (node is CategoryNode categoryNode)
        {
            categoryNode.RecalculateContentLayout();
            return;
        }

        if (node is ILayoutListNode layoutNode)
        {
            layoutNode.RecalculateLayout();

            foreach (var childNode in layoutNode.Nodes)
            {
                RecalculateBottomUp(childNode);
            }

            layoutNode.RecalculateLayout();
        }
    }

    public static void UpdateScrollParams(ScrollingNode<VerticalListNode> scrollingNode)
    {
        UseManualMouseWheel(scrollingNode);
        UpdateScrollParamsCore(scrollingNode);

        if (ManualScrollStates.TryGetValue(scrollingNode, out var state))
        {
            ApplyManualScroll(scrollingNode, state);
        }
    }

    public static void UpdateScrollParams<T>(ScrollingNode<T> scrollingNode)
        where T : NodeBase, new()
        => UpdateScrollParamsCore(scrollingNode);

    public static void UpdateListScrollParams<T, TListItem>(ListNode<T, TListItem> listNode)
        where TListItem : ListItemNode<T>, IListItemNode, new()
    {
        var visibleHeight = Math.Max(1, (int)MathF.Ceiling(listNode.Height));
        var contentHeight = Math.Max(1, (int)MathF.Ceiling(listNode.OptionsList.Count * (TListItem.ItemHeight + listNode.ItemSpacing)));

        listNode.ScrollBarNode.UpdateScrollParams(visibleHeight, Math.Max(contentHeight, visibleHeight));
        DisableScrollbarInput(listNode.ScrollBarNode);
    }

    private static void UpdateScrollParamsCore<T>(ScrollingNode<T> scrollingNode)
        where T : NodeBase, new()
    {
        var barHeight = Math.Max(1, (int)MathF.Ceiling(scrollingNode.ScrollingCollisionNode.Height));
        var contentHeight = Math.Max(1, (int)MathF.Ceiling(scrollingNode.ContentNode.Height));

        scrollingNode.ScrollBarNode.UpdateScrollParams(barHeight, Math.Max(contentHeight, barHeight));
        DisableScrollbarInput(scrollingNode.ScrollBarNode);
    }

    private static unsafe void UseManualMouseWheel(ScrollingNode<VerticalListNode> scrollingNode)
    {
        if (ManualScrollStates.TryGetValue(scrollingNode, out _)) return;

        var state = new ManualScrollState();
        ManualScrollStates.Add(scrollingNode, state);

        var listener = (AtkEventListener*)scrollingNode.ScrollBarNode;
        ((AtkResNode*)scrollingNode.ClippingContentNode)->AtkEventManager.UnregisterEvent(AtkEventType.MouseWheel, 5, listener, false);
        ((AtkResNode*)scrollingNode.ScrollingCollisionNode)->AtkEventManager.UnregisterEvent(AtkEventType.MouseWheel, 5, listener, false);
        ((AtkResNode*)scrollingNode.ContentNode)->AtkEventManager.UnregisterEvent(AtkEventType.MouseWheel, 5, listener, false);

        scrollingNode.ClippingContentNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);
        scrollingNode.ScrollingCollisionNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);
        scrollingNode.ContentNode.AddEvent(AtkEventType.MouseWheel, OnMouseWheel);

        return;

        void OnMouseWheel(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData)
        {
            if (!state.HasOverflow)
            {
                return;
            }

            state.ScrollPosition += atkEventData->IsScrollUp ? -state.ScrollSpeed : state.ScrollSpeed;
            ApplyManualScroll(scrollingNode, state);
            atkEvent->SetEventIsHandled();
        }
    }

    private static void ApplyManualScroll(ScrollingNode<VerticalListNode> scrollingNode, ManualScrollState state)
    {
        var viewportHeight = Math.Max(0.0f, scrollingNode.ScrollingCollisionNode.Height);
        var contentHeight = Math.Max(0.0f, scrollingNode.ContentNode.Height);
        var maxScroll = Math.Max(0, (int)MathF.Ceiling(contentHeight - viewportHeight));

        state.HasOverflow = maxScroll > 0;
        state.ScrollPosition = Math.Clamp(state.ScrollPosition, 0, maxScroll);
        scrollingNode.ContentNode.Y = -state.ScrollPosition;
    }

    private static void DisableScrollbarInput(ScrollBarNode scrollBarNode)
    {
        scrollBarNode.IsAcceptingMouseWheelEvents = false;
        scrollBarNode.RemoveNodeFlags(NodeFlags.RespondToMouse, NodeFlags.HasCollision, NodeFlags.EmitsEvents, NodeFlags.Focusable);
        scrollBarNode.CollisionNode.RemoveNodeFlags(NodeFlags.RespondToMouse, NodeFlags.HasCollision, NodeFlags.EmitsEvents, NodeFlags.Focusable);
    }

    private sealed class ManualScrollState
    {
        public bool HasOverflow;
        public int ScrollPosition;
        public int ScrollSpeed = 36;
    }

    private static readonly ConditionalWeakTable<ScrollingNode<VerticalListNode>, ManualScrollState> ManualScrollStates = new();
}
