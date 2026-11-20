using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.Components.ProgressBars;

namespace NativeMeters.Services;

internal sealed class PartyListMemberBars(int slotCount)
{
    private readonly Slot?[] slots = new Slot?[slotCount];

    public void Attach(ResNode parent, int index, (ProgressBarType Type, bool PreserveEnds) style)
        => EnsureAttached(parent, index, style);

    public void Update(
        ResNode parent,
        int index,
        PartyListMeterSettings settings,
        Combatant combatant,
        Vector2 anchorPosition,
        float progress)
    {
        var node = EnsureAttached(parent, index, settings.MemberBarStyle);
        var progressNode = (IMeterProgressNode)node;

        node.Position = anchorPosition + new Vector2(settings.BarOffsetX, settings.BarOffsetY);
        var size = new Vector2(settings.BarWidth, settings.BarHeight);
        if (node.Size != size)
            node.Size = size;

        progressNode.Progress = progress;
        progressNode.ColorTreatment = settings.BarColorTreatment;
        progressNode.FillRightToLeft = settings.BarFillRightToLeft;
        progressNode.BarColor = settings.BarColorMode is ColorMode.Job or ColorMode.Role
            ? combatant.GetColor(settings.BarColorMode)
            : settings.BarColor;
        progressNode.BackgroundColor = settings.BarBackgroundColor;
        node.IsVisible = true;
        node.MarkDirty();
    }

    public void Hide(int index)
    {
        if ((uint)index < (uint)slots.Length && slots[index] is { } slot)
            slot.Node.IsVisible = false;
    }

    public void Retire()
    {
        foreach (var slot in slots)
            slot?.Node.DisposeLater();

        ClearReferences();
    }

    public void ClearReferences() => Array.Clear(slots);

    private NodeBase EnsureAttached(ResNode parent, int index, (ProgressBarType Type, bool PreserveEnds) style)
    {
        var previous = slots[index];
        if (previous is { } slot && slot.Style == style)
            return slot.Node;

        var node = MeterProgressNodes.Create(style.Type, style.PreserveEnds);
        try
        {
            node.IsVisible = false;
            node.RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision);

            if (node is ComponentGaugeProgressNode gauge)
            {
                gauge.CollisionNode.RemoveNodeFlags(
                    NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision, NodeFlags.Focusable);
            }

            node.AttachNode(parent, NodePosition.AsFirstChild);
        }
        catch
        {
            node.DisposeLater();
            throw;
        }

        previous?.Node.DisposeLater();
        slots[index] = new Slot(node, style);
        return node;
    }

    private readonly record struct Slot(NodeBase Node, (ProgressBarType Type, bool PreserveEnds) Style);
}
