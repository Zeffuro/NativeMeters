using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public class MeterListLayoutNode : OverlayNode
{
    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;
    public override bool HideWithNativeUi => System.Config.General.HideWithNativeUi;

    protected override void OnUpdate()
    {
    }

    private readonly VerticalListNode verticalListNode;
    public MeterListLayoutNode()
    {
        verticalListNode = new VerticalListNode() {
            NodeId = 2,
            X = 0,
            Y = 0,
            Width = 500,
            Height = 500,
            IsVisible = true,
        };
        verticalListNode.AttachNode(this);

        System.ActiveMeterService.CombatDataUpdated += OnCombatDataUpdated;
        RebuildList();
    }

    private void OnCombatDataUpdated()
    {
        RebuildList();
    }

    private void RebuildList() {
        if (!System.ActiveMeterService.HasCombatData()) return;

        uint nodeIndex = 2;
        verticalListNode.SyncWithListData(System.ActiveMeterService.GetCombatants(), node => node.Combatant, data => new MeterRowNode {
            NodeId = nodeIndex++,
            Height = 36.0f,
            Width = verticalListNode.Width,
            IsVisible = true,
            Encounter = System.ActiveMeterService.GetEncounter(),
            Combatant = data
        });

        foreach (var meterRowNode in verticalListNode.GetNodes<MeterRowNode>())
        {
            meterRowNode.Update();
        }

        verticalListNode.ReorderNodes((x, y) => ComparisonBy(x, y, c => c.ENCDPS));
    }

    private static int ComparisonBy<T>(NodeBase x, NodeBase y, Func<Combatant, T> selector, bool ascending = false) where T : IComparable<T>
    {
        if (x is not MeterRowNode left || y is not MeterRowNode right)
            return 0;

        var leftValue = selector(left.Combatant);
        var rightValue = selector(right.Combatant);

        int result = Comparer<T>.Default.Compare(leftValue, rightValue);
        return ascending ? result : -result;
    }

    public void OnDispose(bool disposing)
    {
        System.ActiveMeterService.CombatDataUpdated -= OnCombatDataUpdated;
    }
}