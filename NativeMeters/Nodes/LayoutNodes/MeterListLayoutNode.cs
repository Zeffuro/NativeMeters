using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.UiOverlay;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Rendering;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterListLayoutNode : OverlayNode
{
    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;
    public override bool HideWithNativeUi => System.Config.Visibility.HideWithNativeUi;

    private IMeterService? hookedService;
    private bool isDisposing;
    private bool isPreWarmed;
    private bool isAttached;
    private bool hasPendingRecreate;

    private List<CombatantRowData>? cachedOptionsList;
    private List<CombatantRowData> selfOptionsList = [];
    private float listRowHeight;
    private bool wasCompact;
    private RowState? lastRowState;
    private readonly record struct RowState(string Stat, int MaxRows, bool LimitBreak, bool NonPlayers, bool PinSelf);

    private record StructuralState(bool Clickthrough, float RowHeight, int MaxRows, int StructureHash);
    private StructuralState? lastState;

    private MeterBackgroundNode? backgroundNode;
    private StaticComponentContainerNode? headerContainer;
    private StaticComponentContainerNode? footerContainer;
    private ListNode<CombatantRowData, MeterRowListItemNode> listNode = null!;

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
        OnResizeComplete = node =>
        {
            if (MeterSettings is { IsCollapsed: false }) MeterSettings.Size = node.Size;
        };

        SubscribeToCombatDataUpdates();
        Scale = new Vector2(2);
    }

    public void SubscribeToCombatDataUpdates()
    {
        if (hookedService != null)
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;

        hookedService = System.ActiveMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
    }

    private void InitializeFromSettings()
    {
        if (MeterSettings == null) return;

        Position = MeterSettings.Position;
        Size = MeterRowBuilder.GetDisplaySize(MeterSettings, selfOptionsList.Count != 0);

        int currentHash = CalculateStructureHash();
        var currentState = new StructuralState(MeterSettings.IsClickthrough, MeterSettings.RowHeight, MeterSettings.MaxCombatants, currentHash);

        if (lastState != currentState)
        {
            hasPendingRecreate = true;
            lastState = currentState;
        }
    }

    private int CalculateStructureHash()
    {
        if (MeterSettings == null) return 0;
        HashCode hash = new();
        foreach (var c in MeterSettings.HeaderComponents) AddComponentStructureHash(ref hash, c);
        foreach (var c in MeterSettings.RowComponents) AddComponentStructureHash(ref hash, c);
        foreach (var c in MeterSettings.FooterComponents) AddComponentStructureHash(ref hash, c);
        return hash.ToHashCode();
    }

    private static void AddComponentStructureHash(ref HashCode hash, ComponentSettings component)
    {
        hash.Add(component.Id);
        hash.Add(component.ZIndex);
        hash.Add(component.Type);
    }

    private void RecreateList()
    {
        if (MeterSettings == null) return;
        if (!isAttached) return;

        isPreWarmed = false;

        backgroundNode.DisposeLater();
        headerContainer.DisposeLater();
        footerContainer.DisposeLater();
        listNode.DisposeLater();

        backgroundNode = new MeterBackgroundNode {
            Size = Size,
            Style = MeterSettings.BackgroundStyle,
            BackgroundColor = MeterSettings.WindowColor,
            IsVisible = MeterSettings.ShowWindowBackground
        };
        backgroundNode.AttachNode(this);

        headerContainer = new StaticComponentContainerNode(MeterSettings.HeaderComponents) { MeterSettings = MeterSettings };
        headerContainer.AttachNode(this);

        footerContainer = new StaticComponentContainerNode(MeterSettings.FooterComponents) { MeterSettings = MeterSettings };
        footerContainer.AttachNode(this);

        MeterRowListItemNode.HeightHint = MeterSettings.RowHeight;
        listRowHeight = MeterSettings.RowHeight;

        listNode = new ListNode<CombatantRowData, MeterRowListItemNode> {
            ItemSpacing = MeterSettings.RowSpacing,
            AutoResetScroll = false,
            OptionsList = [],
        };

        MeterRowListItemNode.PrebuildSettings = null;

        if (MeterSettings.IsClickthrough)
        {
            listNode.RemoveEvent(AtkEventType.MouseWheel);
            listNode.RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision);
        }

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
        }

        if (!isAttached) return;

        if (hasPendingRecreate)
        {
            hasPendingRecreate = false;
            RecreateList();
            return;
        }

        if (!isPreWarmed) return;

        if (lastRowState != GetRowState()) RebuildList();
        UpdateDisplayedRows();
        var compact = MeterSettings.IsCollapsed && MeterSettings.KeepSelfVisibleWhenCollapsed;
        if (compact || wasCompact)
        {
            var displaySize = MeterRowBuilder.GetDisplaySize(MeterSettings, selfOptionsList.Count != 0);
            if (Size != displaySize) Size = displaySize;
        }
        wasCompact = compact;

        bool hasActiveData = System.ActiveMeterService.HasCombatData();
        bool isEditing = !MeterSettings.IsLocked || System.Config.General.PreviewEnabled;
        bool hiddenByVisibility = !isEditing && VisibilityEvaluator.ShouldHide();
        IsVisible = MeterSettings.IsEnabled && !hiddenByVisibility && (hasActiveData || isEditing || MeterSettings.IsCollapsed);
        EnableMoving = !MeterSettings.IsLocked;
        EnableResizing = !MeterSettings.IsLocked && !MeterSettings.IsCollapsed;

        if (!IsVisible) return;

        var scaleFactor = MeterSettings.Scale / 100f;
        Scale = new Vector2(scaleFactor);

        float headerH = MeterSettings.HeaderEnabled ? MeterSettings.HeaderHeight : 0;
        float footerH = (MeterSettings.FooterEnabled && !MeterSettings.IsCollapsed) ? MeterSettings.FooterHeight : 0;
        var contentInset = MeterSettings.ShowWindowBackground
            && MeterSettings.BackgroundStyle is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused
                ? 8.0f : 0.0f;
        var contentWidth = Math.Max(0, Width - contentInset * 2);

        if (backgroundNode != null)
        {
            backgroundNode.IsVisible = MeterSettings.ShowWindowBackground;
            backgroundNode.Style = MeterSettings.BackgroundStyle;
            backgroundNode.BackgroundColor = MeterSettings.WindowColor;
            if (backgroundNode.Size != Size) backgroundNode.Size = Size;
        }

        if (headerContainer != null)
        {
            headerContainer.IsVisible = MeterSettings.HeaderEnabled;
            var headerSize = new Vector2(contentWidth, headerH);
            if (headerContainer.Size != headerSize) headerContainer.Size = headerSize;
            headerContainer.Position = new Vector2(contentInset,
                MeterSettings.IsCollapsed && !MeterSettings.KeepSelfVisibleWhenCollapsed ? 0 : contentInset);
            if (headerContainer.IsVisible) headerContainer.Update();
        }

        if (footerContainer != null)
        {
            footerContainer.IsVisible = MeterSettings.FooterEnabled && !MeterSettings.IsCollapsed;
            var footerSize = new Vector2(contentWidth, footerH);
            if (footerContainer.Size != footerSize) footerContainer.Size = footerSize;
            footerContainer.Position = new Vector2(contentInset, Height - footerH - contentInset);
            if (footerContainer.IsVisible) footerContainer.Update();
        }

        if (listNode != null)
        {
            listNode.IsVisible = !MeterSettings.IsCollapsed
                || (MeterSettings.KeepSelfVisibleWhenCollapsed && selfOptionsList.Count != 0);
            listNode.Position = new Vector2(contentInset, headerH + contentInset);

            var desiredListSize = new Vector2(contentWidth, Math.Max(0, Height - headerH - footerH - contentInset * 2));
            ResizeListNodeSafely(desiredListSize);

            if (Math.Abs(listNode.ItemSpacing - MeterSettings.RowSpacing) > 0.1f)
                UpdateListItemSpacingSafely(MeterSettings.RowSpacing);

            UpdateScrollbarLayout();
            if (listNode.IsVisible) listNode.Update();
        }
    }

    private void UpdateScrollbarLayout()
    {
        if (MeterSettings == null || listNode == null) return;

        var showScrollbar = MeterSettings.ShowScrollbar && !MeterSettings.IsClickthrough
            && listNode.OptionNodes.Count > 0 && listNode.OptionsList.Count > listNode.OptionNodes.Count;
        listNode.ScrollBarNode.IsVisible = showScrollbar;

        var rowWidth = Math.Max(0, listNode.Width - (showScrollbar ? 16.0f : 0.0f));
        foreach (var row in listNode.OptionNodes)
        {
            if (row.Width != rowWidth)
                row.Width = rowWidth;
        }
    }

    private void ResizeListNodeSafely(Vector2 desiredSize)
    {
        if (listNode.Size == desiredSize)
            return;

        var oldRowCount = GetListNodeRowCount(listNode.Height, listNode.ItemSpacing);
        var newRowCount = GetListNodeRowCount(desiredSize.Y, listNode.ItemSpacing);

        if (oldRowCount != newRowCount)
        {
            listNode.DetachNode();
            listNode.Size = desiredSize;
            listNode.AttachNode(this);
            return;
        }

        listNode.Size = desiredSize;
    }

    private void UpdateListItemSpacingSafely(float itemSpacing)
    {
        listNode.DetachNode();
        listNode.ItemSpacing = itemSpacing;
        listNode.AttachNode(this);
    }

    private int GetListNodeRowCount(float height, float itemSpacing)
        => MeterRowBuilder.GetVisibleRowCount(height, listRowHeight, itemSpacing);

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

        var rows = MeterRowBuilder.Build(combatants, MeterSettings, !hasCombat || System.Config.General.PreviewEnabled);
        cachedOptionsList = rows.Expanded;
        selfOptionsList = rows.Self;
        lastRowState = GetRowState();
        UpdateDisplayedRows();
    }

    private RowState GetRowState() => new(MeterSettings!.StatToTrack, MeterSettings.MaxCombatants,
        MeterSettings.ShowLimitBreak, MeterSettings.ShowNonPlayerCombatants, MeterSettings.PinSelfToTop);

    private void UpdateDisplayedRows()
    {
        if (MeterSettings == null || listNode == null || cachedOptionsList == null) return;
        var rows = MeterSettings.IsCollapsed && MeterSettings.KeepSelfVisibleWhenCollapsed
            ? selfOptionsList : cachedOptionsList;
        if (ReferenceEquals(listNode.OptionsList, rows)) return;
        var resetScroll = listNode.OptionsList.Count > rows.Count;
        listNode.OptionsList = rows;
        if (resetScroll) listNode.ResetScroll();
        UpdateScrollbarLayout();
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
