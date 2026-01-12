using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterListLayoutNode : OverlayNode
{
    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;
    public override bool HideWithNativeUi => System.Config.General.HideWithNativeUi;

    private IMeterService? hookedService;
    private bool isDisposing;

    public required MeterSettings? MeterSettings
    {
        get;
        set
        {
            field = value;
            if (value != null) InitializeFromSettings();
        }
    }

    private readonly VerticalListNode verticalListNode;

    public MeterListLayoutNode()
    {
        verticalListNode = new VerticalListNode {
            DisableCollisionNode = true,
            X = 0,
            Y = 0,
            Width = 500,
            Height = 300,
            IsVisible = true,
        };
        verticalListNode.AttachNode(this);

        hookedService = System.ActiveMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
    }

    private void InitializeFromSettings()
    {
        if (MeterSettings == null) return;

        Position = MeterSettings.Position;
        Size = MeterSettings.Size;

        OnMoveComplete = node => MeterSettings.Position = node.Position;
        OnResizeComplete = node => MeterSettings.Size = node.Size;

        RebuildList();
    }

    protected override void OnUpdate()
    {
        DisableCollisionNode = true;
        if (MeterSettings == null) return;

        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked;
        IsVisible = MeterSettings.IsEnabled;

        if (verticalListNode.Size != Size)
        {
            verticalListNode.Size = Size;
        }
    }

    private void OnCombatDataUpdated()
    {
        if (isDisposing) return;
        RebuildList();
    }

    private void RebuildList() {
        if (MeterSettings == null) return;
        if (!System.ActiveMeterService.HasCombatData()) return;

        var combatants = System.ActiveMeterService.GetCombatants();

        if (!MeterSettings.ShowLimitBreak) {
            combatants = combatants.Where(c => !c.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase));
        }

        verticalListNode.SyncWithListData(combatants.Take(MeterSettings.MaxCombatants), node => node.Combatant, data => new MeterRowNode {
            Height = 36.0f,
            Width = verticalListNode.Width,
            IsVisible = true,
            MeterSettings = MeterSettings,
            Encounter = System.ActiveMeterService.GetEncounter(),
            Combatant = data,
        });

        foreach (var meterRowNode in verticalListNode.GetNodes<MeterRowNode>())
        {
            meterRowNode.Update();
        }

        var selector = CombatantStatHelpers.GetStatSelector(MeterSettings.StatToTrack);
        verticalListNode.ReorderNodes((x, y) => ComparisonBy(x, y, selector));
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

    public void OnDispose()
    {
        isDisposing = true;

        if (hookedService != null)
        {
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;
            hookedService = null;
        }

        verticalListNode.Clear();
    }
}