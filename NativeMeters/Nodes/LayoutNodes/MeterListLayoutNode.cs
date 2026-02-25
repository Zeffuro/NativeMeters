using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
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
    private bool isPreWarmed;
    private bool isAttached;

    private record StructuralState(bool Clickthrough, float RowHeight, int MaxRows, int StructureHash);    private StructuralState? lastState;

    private MeterBackgroundNode? backgroundNode;
    private StaticComponentContainerNode? headerContainer;
    private StaticComponentContainerNode? footerContainer;
    private ListNode<CombatantRowData, MeterRowListItemNode> listNode;

    public void UpdateSettings() => InitializeFromSettings();

    public required MeterSettings? MeterSettings
    {
        get;
        set
        {
            field = value;
            if (value != null && isAttached) InitializeFromSettings();
        }
    }

    public MeterListLayoutNode()
    {
        OnMoveComplete = node => MeterSettings!.Position = node.Position;
        OnResizeComplete = node => MeterSettings!.Size = node.Size;

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

        Position = MeterSettings.Position;
        Size = MeterSettings.Size;

        int currentHash = CalculateStructureHash();
        var currentState = new StructuralState(MeterSettings.IsClickthrough, MeterSettings.RowHeight, MeterSettings.MaxCombatants, currentHash);

        if (lastState != currentState)
        {
            RecreateList();
            lastState = currentState;
        }
    }

    private int CalculateStructureHash()
    {
        if (MeterSettings == null) return 0;
        HashCode hash = new();
        foreach (var c in MeterSettings.HeaderComponents) { hash.Add(c.Id); hash.Add(c.ZIndex); }
        foreach (var c in MeterSettings.RowComponents) { hash.Add(c.Id); hash.Add(c.ZIndex); }
        foreach (var c in MeterSettings.FooterComponents) { hash.Add(c.Id); hash.Add(c.ZIndex); }
        return hash.ToHashCode();
    }

    private void RecreateList()
    {
        if (MeterSettings == null) return;
        if (!isAttached) return;

        isPreWarmed = false;

        backgroundNode?.Dispose();
        headerContainer?.Dispose();
        footerContainer?.Dispose();
        listNode?.Dispose();

        backgroundNode = new MeterBackgroundNode {
            Size = Size,
            BackgroundColor = MeterSettings.WindowColor,
            IsVisible = MeterSettings.ShowWindowBackground
        };
        backgroundNode.AttachNode(this);

        headerContainer = new StaticComponentContainerNode(MeterSettings.HeaderComponents) { MeterSettings = MeterSettings };
        headerContainer.AttachNode(this);

        footerContainer = new StaticComponentContainerNode(MeterSettings.FooterComponents) { MeterSettings = MeterSettings };
        footerContainer.AttachNode(this);

        MeterRowListItemNode.HeightHint = MeterSettings.RowHeight;
        listNode = new ListNode<CombatantRowData, MeterRowListItemNode> {
            ItemSpacing = MeterSettings.RowSpacing,
            OptionsList = []
        };

        if (MeterSettings.IsClickthrough)
        {
            DisableCollisionNode = true;
            listNode.DisableCollisionNode = true;
        }

        listNode.ScrollBarNode.IsVisible = false;
        listNode.ScrollBarNode.IsEnabled = false;
        listNode.AttachNode(this);

        RebuildList();
        listNode.Update();

        isPreWarmed = true;
    }

    protected override void OnUpdate()
    {
        if (MeterSettings == null) return;

        if (!isAttached)
        {
            isAttached = true;
            InitializeFromSettings();
            return;
        }

        if (!isPreWarmed) return;

        bool hasActiveData = System.ActiveMeterService.HasCombatData();
        bool isEditing = !MeterSettings.IsLocked || System.Config.General.PreviewEnabled;
        IsVisible = MeterSettings.IsEnabled && (hasActiveData || isEditing || MeterSettings.IsCollapsed);
        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked && !MeterSettings.IsCollapsed;

        float headerH = MeterSettings.HeaderEnabled ? MeterSettings.HeaderHeight : 0;
        float footerH = (MeterSettings.FooterEnabled && !MeterSettings.IsCollapsed) ? MeterSettings.FooterHeight : 0;

        if (backgroundNode != null)
        {
            backgroundNode.IsVisible = MeterSettings.ShowWindowBackground;
            backgroundNode.BackgroundColor = MeterSettings.WindowColor;
            backgroundNode.Size = Size;
        }

        if (headerContainer != null)
        {
            headerContainer.IsVisible = MeterSettings.HeaderEnabled;
            headerContainer.Size = new Vector2(Width, headerH);
            headerContainer.Position = Vector2.Zero;
            headerContainer.Update();
        }

        if (footerContainer != null)
        {
            footerContainer.IsVisible = MeterSettings.FooterEnabled && !MeterSettings.IsCollapsed;
            footerContainer.Size = new Vector2(Width, footerH);
            footerContainer.Position = new Vector2(0, Height - footerH);
            footerContainer.Update();
        }

        if (listNode != null)
        {
            listNode.IsVisible = !MeterSettings.IsCollapsed;
            listNode.Position = new Vector2(0, headerH);
            listNode.Size = new Vector2(Width, Math.Max(0, Height - headerH - footerH));

            if (Math.Abs(listNode.ItemSpacing - MeterSettings.RowSpacing) > 0.1f)
                listNode.ItemSpacing = MeterSettings.RowSpacing;

            listNode.Update();
        }
    }

    private void OnCombatDataUpdated()
    {
        if (isDisposing) return;
        RebuildList();
    }

    private void RebuildList()
    {
        if (MeterSettings == null || listNode == null) return;

        var hasCombat = System.ActiveMeterService.HasCombatData();
        var combatants = hasCombat
            ? System.ActiveMeterService.GetCombatants().ToList()
            : FakeCombatantFactory.CreateFixedCombatants(MeterSettings.MaxCombatants);

        if (!MeterSettings.ShowLimitBreak) {
            combatants.RemoveAll(combatant => combatant.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase));
        }

        if (!MeterSettings.ShowNonPlayerCombatants) {
            combatants.RemoveAll(combatant => combatant.Job.RowId == 0
                                              && !combatant.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase));
        }

        var selector = StatSelector.GetStatSelector(MeterSettings.StatToTrack);
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
