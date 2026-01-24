using System;
using System.Linq;
using System.Numerics;
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

    private ListNode<CombatantRowData, MeterRowListItemNode> listNode;

    public MeterListLayoutNode()
    {
        MeterRowListItemNode.HeightHint = MeterSettings?.RowHeight ?? 36.0f;

        listNode = new ListNode<CombatantRowData, MeterRowListItemNode> {
            X = 0, Y = 0,
            Position = Vector2.Zero,
            Size = new Vector2(500, 300),
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
        headerContainer = new StaticComponentContainerNode(MeterSettings.HeaderComponents);
        headerContainer.AttachNode(this);

        footerContainer?.Dispose();
        footerContainer = new StaticComponentContainerNode(MeterSettings.FooterComponents);
        footerContainer.AttachNode(this);

        OnMoveComplete = node => MeterSettings.Position = node.Position;
        OnResizeComplete = node => MeterSettings.Size = node.Size;

        RecreateList();
    }

    public void RecreateList()
    {
        if (MeterSettings == null) return;
        listNode?.Dispose();
        MeterRowListItemNode.HeightHint = MeterSettings.RowHeight;
        listNode = new ListNode<CombatantRowData, MeterRowListItemNode> {
            ItemSpacing = MeterSettings.RowSpacing,
            OptionsList = []
        };

        if (MeterSettings.IsClickthrough)
        {
            listNode.DisableCollisionNode = true;
        }

        listNode.ScrollBarNode.IsVisible = false;
        listNode.ScrollBarNode.IsEnabled = false;

        listNode.AttachNode(this);

        RebuildList();
    }

    protected override void OnUpdate()
    {
        if (MeterSettings == null) return;

        bool hasActiveData = System.ActiveMeterService.HasCombatData();
        bool isEditing = !MeterSettings.IsLocked || System.Config.General.PreviewEnabled;

        IsVisible = MeterSettings.IsEnabled && (hasActiveData || isEditing);

        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked;

        float currentHeaderHeight = MeterSettings.HeaderEnabled ? MeterSettings.HeaderHeight : 0;
        float currentFooterHeight = MeterSettings.FooterEnabled ? MeterSettings.FooterHeight : 0;

        headerContainer?.IsVisible = MeterSettings.HeaderEnabled;
        headerContainer?.Position = Vector2.Zero;
        headerContainer?.Size = new Vector2(Width, currentHeaderHeight);

        footerContainer?.IsVisible = MeterSettings.FooterEnabled;
        footerContainer?.Position = new Vector2(0, Height - currentFooterHeight);
        footerContainer?.Size = new Vector2(Width, currentFooterHeight);

        listNode.Position = new Vector2(0, currentHeaderHeight);
        listNode.Size = new Vector2(Width, Math.Max(0, Height - currentHeaderHeight - currentFooterHeight));
        if(Math.Abs(listNode.ItemSpacing - MeterSettings.RowSpacing) > 0.1) listNode.ItemSpacing = MeterSettings.RowSpacing;

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
