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

    private StaticComponentContainerNode? headerContainer;
    private StaticComponentContainerNode? footerContainer;

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

        SubscribeToCombatDataUpdates();
    }

    public void SubscribeToCombatDataUpdates()
    {
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

        headerContainer?.Dispose();
        footerContainer?.Dispose();

        headerContainer = new StaticComponentContainerNode(MeterSettings.HeaderComponents);
        headerContainer.AttachNode(this);

        footerContainer = new StaticComponentContainerNode(MeterSettings.FooterComponents);
        footerContainer.AttachNode(this);

        OnMoveComplete = node => MeterSettings.Position = node.Position;
        OnResizeComplete = node => MeterSettings.Size = node.Size;

        RebuildList();
    }

    protected override void OnUpdate()
    {
        if (MeterSettings == null) return;

        bool hasActiveData = System.ActiveMeterService.HasCombatData() || System.Config.General.PreviewEnabled;
        bool isEditing = !MeterSettings.IsLocked;

        IsVisible = MeterSettings.IsEnabled && (hasActiveData || isEditing);

        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked;

        if (listNode.Size != Size)
        {
            listNode.Size = Size;
        }

        headerContainer?.Update();
        footerContainer?.Update();
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

        if (combatants.Count == 0 && !System.Config.General.PreviewEnabled)
        {
            combatants = FakeCombatantFactory.CreateFixedCombatants(MeterSettings.MaxCombatants);
        }

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

        if (hookedService == null) return;

        hookedService.CombatDataUpdated -= OnCombatDataUpdated;
        hookedService = null;
    }
}
