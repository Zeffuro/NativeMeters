using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterListLayoutNode : SimpleComponentNode
{
    private IMeterService meterService;
    private VerticalListNode verticalListNode;
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
        Service.NativeController.AttachNode(verticalListNode, this);

        meterService = Service.ActiveMeterService;
        meterService.CombatDataUpdated += OnCombatDataUpdated;
        RebuildList();
    }

    private void OnCombatDataUpdated()
    {
        RebuildList();
    }

    private void RebuildList() {
        if (meterService.CurrentCombatData is null) return;
        var combatDataMessage = meterService.CurrentCombatData;
        Service.Logger.Info(combatDataMessage.Encounter.ENCDPS.ToString());

        uint nodeIndex = 2;
        verticalListNode.SyncWithListData(combatDataMessage.Combatant.Values, node => node.Combatant, data => new MeterRowNode {
            NodeId = nodeIndex++,
            Height = 36.0f,
            Width = verticalListNode.Width,
            IsVisible = true,
            Encounter = combatDataMessage.Encounter,
            Combatant = data
        });

        verticalListNode.ReorderNodes((x, y) => ComparisonBy(x, y, c => c.ENCDPS));
    }

    private static int Comparison(NodeBase x, NodeBase y) {
        if (x is not MeterRowNode left ||  y is not MeterRowNode right) return 0;

        return string.Compare(left.Combatant.Name, right.Combatant.Name, StringComparison.Ordinal);
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

    protected override void Dispose(bool disposing)
    {
        verticalListNode?.Dispose();
        verticalListNode = null!;
        meterService.CombatDataUpdated -= OnCombatDataUpdated;
    }
}