using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public record CombatantRowData(Combatant Combatant, MeterSettings Settings);

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

    private readonly ListNode<CombatantRowData, MeterRowListItemNode> listNode;

    public MeterListLayoutNode()
    {
        listNode = new ListNode<CombatantRowData, MeterRowListItemNode> {
            X = 0, Y = 0,
            Width = 500, Height = 300,
            ItemSpacing = 0.0f,
            OptionsList = []
        };

        if (MeterSettings?.IsClickthrough == true)
        {
            listNode.DisableCollisionNode = true;
        }

        listNode.AttachNode(this);

        listNode.ScrollBarNode.IsVisible = false;
        listNode.ScrollBarNode.IsEnabled = false;

        hookedService = System.ActiveMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
    }

    private void InitializeFromSettings()
    {
        if (MeterSettings == null) return;

        if (MeterSettings.IsClickthrough)
        {
            DisableCollisionNode = true;
            listNode.DisableCollisionNode = true;
        }

        Position = MeterSettings.Position;
        Size = MeterSettings.Size;

        OnMoveComplete = node => MeterSettings.Position = node.Position;
        OnResizeComplete = node => MeterSettings.Size = node.Size;

        RebuildList();
    }

    protected override void OnUpdate()
    {
        if (MeterSettings == null) return;

        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked;
        IsVisible = MeterSettings.IsEnabled;

        if (listNode.Size != Size)
        {
            listNode.Size = Size;
        }

        listNode.Update();
    }

    private void OnCombatDataUpdated()
    {
        if (isDisposing) return;
        RebuildList();
    }

    private void RebuildList() {
        if (MeterSettings == null) return;

        var combatants = System.ActiveMeterService.GetCombatants().ToList();

        if (!MeterSettings.ShowLimitBreak) {
            combatants.RemoveAll(combatant => combatant.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase));
        }

        var selector = CombatantStatHelpers.GetStatSelector(MeterSettings.StatToTrack);
        combatants.Sort((left, right) => selector(right).CompareTo(selector(left)));

        listNode.OptionsList = combatants
            .Take(MeterSettings.MaxCombatants)
            .Select(combatant => new CombatantRowData(combatant, MeterSettings))
            .ToList();
    }

    public void OnDispose()
    {
        isDisposing = true;

        if (hookedService != null)
        {
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;
            hookedService = null;
        }
    }
}