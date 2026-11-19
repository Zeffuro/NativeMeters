using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.UiOverlay;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Rendering;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterListLayoutNode : OverlayNode
{
    private const float EditMargin = 16;
    private const float LockButtonGap = 2;
    private static readonly Vector2 LockButtonSize = new(64, 24);

    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;
    public override bool HideWithNativeUi => System.Config.Visibility.HideWithNativeUi;

    private Vector2 positionBeforeResize;
    private Vector2 resizeRemainder;

    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            if (EnableResizing) positionBeforeResize = base.Position;
            base.Position = value;
        }
    }

    public override Vector2 Size
    {
        get => base.Size;
        set
        {
            if (EnableResizing) ResizeFromViewport(value - base.Size);
            else SetDisplaySize(value);
        }
    }

    private IMeterService? hookedService;
    private Encounter? placementEncounter;
    private MeterPlacementSession? placement;
    private bool isDisposing;
    private bool isPreWarmed;
    private bool isAttached;
    private bool hasPendingRecreate;
    private int rowsDirty;

    private List<CombatantRowData>? cachedOptionsList;
    private List<CombatantRowData> selfOptionsList = [];
    private float listRowHeight;
    private bool wasCompact;
    private RowState? lastRowState;
    private readonly record struct RowState(string Stat, int MaxRows, bool LimitBreak, bool NonPlayers, bool PinSelf);

    private record StructuralState(MeterLayout Layout, bool Clickthrough, float RowHeight, int MaxRows, int StructureHash);
    private StructuralState? lastState;

    private MeterBackgroundNode? backgroundNode;
    private ResNode? bodyLayer;
    private TextButtonNode? lockButton;
    private bool editModeRetiring;
    private bool editGeometryChanged;
    private nint editParent;
    private StaticComponentContainerNode? headerContainer;
    private StaticComponentContainerNode? footerContainer;
    private ListNode<CombatantRowData, MeterRowListItemNode>? listNode;
    private MeterCardStripNode? cardStrip;

    public void UpdateSettings() => InitializeFromSettings();
    private unsafe nint HudParent => (nint)((AtkResNode*)this)->ParentNode;
    internal bool IsAttachedToHud => HudParent != 0;
    internal bool IsEditModeReady => EnableMoving && editParent == HudParent
        && (NodeFlags & NodeFlags.Visible) != 0;

    internal MeterPlacementSession? Placement
    {
        get => placement;
        set
        {
            if (ReferenceEquals(placement, value)) return;
            placement = value;
            if (isAttached) InitializeFromSettings();
            Interlocked.Exchange(ref rowsDirty, 1);
        }
    }

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
        OnMoveComplete = node =>
        {
            if (!IsEditModeReady) return;
            if (placement != null) placement.Position = node.Position;
            else if (MeterSettings is { IsLocked: false }) MeterSettings.Position = node.Position;
        };
        OnResizeComplete = node =>
        {
            resizeRemainder = Vector2.Zero;
            if (!IsEditModeReady || MeterSettings is not { IsCollapsed: false }) return;
            if (placement != null)
            {
                placement.Position = node.Position;
                placement.Size = node.Size;
                return;
            }
            if (!MeterSettings.IsLocked)
            {
                MeterSettings.Size = node.Size;
                MeterSettings.Position = node.Position;
            }
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
        Interlocked.Exchange(ref rowsDirty, 1);
    }

    private void InitializeFromSettings()
    {
        if (MeterSettings == null) return;

        Position = placement?.Position ?? MeterSettings.Position;
        SetDisplaySize(placement?.Size ?? MeterRowBuilder.GetDisplaySize(MeterSettings, selfOptionsList.Count != 0));

        int currentHash = CalculateStructureHash();
        var currentState = new StructuralState(MeterSettings.Layout, MeterSettings.IsClickthrough,
            MeterSettings.RowHeight, MeterSettings.MaxCombatants, currentHash);

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
        bodyLayer.DisposeLater();
        listNode = null;
        cardStrip = null;

        backgroundNode = new MeterBackgroundNode {
            Size = Size,
            Style = MeterSettings.BackgroundStyle,
            BackgroundColor = MeterSettings.WindowColor,
            IsVisible = MeterSettings.ShowWindowBackground
        };
        backgroundNode.AttachNode(this);

        headerContainer = new StaticComponentContainerNode(MeterSettings.HeaderComponents) { MeterSettings = MeterSettings };
        footerContainer = new StaticComponentContainerNode(MeterSettings.FooterComponents) { MeterSettings = MeterSettings };

        bodyLayer = new ResNode();
        bodyLayer.AttachNode(this);

        if (MeterSettings.Layout == MeterLayout.Horizontal)
        {
            cardStrip = new MeterCardStripNode();
            cardStrip.AttachNode(bodyLayer);
        }
        else
        {
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

            listNode.AttachNode(bodyLayer);
        }

        headerContainer.AttachNode(this);
        footerContainer.AttachNode(this);

        RebuildList();
        UpdateDisplayedRows();
        listNode?.Update();

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
        bool hasActiveData = System.ActiveMeterService.HasCombatData();
        bool canMove = placement != null || !MeterSettings.IsLocked;
        bool isEditing = canMove || System.Config.General.PreviewEnabled;
        bool hiddenByVisibility = !isEditing && VisibilityEvaluator.ShouldHide();
        IsVisible = MeterSettings.IsEnabled && !hiddenByVisibility && (hasActiveData || isEditing || MeterSettings.IsCollapsed);
        if (Volatile.Read(ref rowsDirty) != 0 || lastRowState != GetRowState()) RebuildList();
        UpdateDisplayedRows();
        var compact = MeterSettings.IsCollapsed
            && (MeterSettings.KeepSelfVisibleWhenCollapsed || MeterSettings.Layout == MeterLayout.Horizontal);
        if (compact || wasCompact)
        {
            var displaySize = MeterRowBuilder.GetDisplaySize(MeterSettings, selfOptionsList.Count != 0);
            SetDisplaySize(displaySize);
        }
        wasCompact = compact;

        var scaleFactor = MeterSettings.Scale / 100f;
        Scale = new Vector2(scaleFactor);
        UpdateEditMode(IsVisible && canMove && (NodeFlags & NodeFlags.Visible) != 0, !MeterSettings.IsCollapsed);
        UpdateLockButton();
        if (!IsVisible) return;

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
                MeterSettings.IsCollapsed && !compact ? 0 : contentInset);
            if (headerContainer.IsVisible) headerContainer.Update(placementEncounter);
        }

        if (footerContainer != null)
        {
            footerContainer.IsVisible = MeterSettings.FooterEnabled && !MeterSettings.IsCollapsed;
            var footerSize = new Vector2(contentWidth, footerH);
            if (footerContainer.Size != footerSize) footerContainer.Size = footerSize;
            footerContainer.Position = new Vector2(contentInset, Height - footerH - contentInset);
            if (footerContainer.IsVisible) footerContainer.Update(placementEncounter);
        }

        var rowOrigin = new Vector2(contentInset, headerH + contentInset);
        var desiredListSize = new Vector2(contentWidth, Math.Max(0, Height - headerH - footerH - contentInset * 2));
        var showRows = !MeterSettings.IsCollapsed
            || (MeterSettings.KeepSelfVisibleWhenCollapsed && selfOptionsList.Count != 0);

        if (cardStrip != null)
        {
            cardStrip.IsVisible = showRows;
            if (showRows && cachedOptionsList != null)
            {
                cardStrip.Position = rowOrigin;
                if (cardStrip.Size != desiredListSize) cardStrip.Size = desiredListSize;
                cardStrip.Update(MeterSettings, MeterSettings.IsCollapsed ? selfOptionsList : cachedOptionsList);
            }
        }

        if (listNode != null)
        {
            listNode.IsVisible = showRows;
            listNode.Position = rowOrigin;

            ResizeListNodeSafely(listNode, desiredListSize);

            if (Math.Abs(listNode.ItemSpacing - MeterSettings.RowSpacing) > 0.1f)
                UpdateListItemSpacingSafely(listNode, MeterSettings.RowSpacing);

            UpdateScrollbarLayout();
            if (listNode.IsVisible) listNode.Update();
        }
    }

    private void SetDisplaySize(Vector2 size)
    {
        if (MeterSettings?.Layout == MeterLayout.Horizontal)
            size = Vector2.Clamp(size, Vector2.Zero, new Vector2(ushort.MaxValue - EditMargin * 2));
        var previous = base.Size;
        if (previous == size) return;
        base.Size = size;
        if (EnableMoving && base.Size != previous) editGeometryChanged = true;
    }

    private void ResizeFromViewport(Vector2 delta)
    {
        var scale = Scale.X;
        if (!float.IsFinite(scale) || scale <= 0) return;

        var previous = base.Size;
        var requested = Vector2.Clamp(previous + delta / scale + resizeRemainder,
            Vector2.One, new Vector2(ushort.MaxValue - EditMargin * 2));
        base.Size = requested;
        resizeRemainder = requested - base.Size;

        var positionDelta = base.Position - positionBeforeResize;
        var acceptedDelta = (previous - base.Size) * scale;
        base.Position = positionBeforeResize + new Vector2(
            positionDelta.X == 0 ? 0 : acceptedDelta.X,
            positionDelta.Y == 0 ? 0 : acceptedDelta.Y);
    }

    private void UpdateEditMode(bool enabled, bool allowResize)
    {
        if (editModeRetiring) return;
        var parent = HudParent;
        if (EnableMoving)
        {
            if (enabled && EnableResizing == allowResize && !editGeometryChanged && editParent == parent) return;
            editModeRetiring = true;
            StopEditing();
            System.OverlayManager.QueueMeterRefresh(this);
            return;
        }

        if (!enabled || parent == 0) return;
        editParent = parent;
        EnableMoving = true;
        if (allowResize) EnableResizing = true;
    }

    internal void CheckEditAttachment()
    {
        if (EnableMoving && HudParent != editParent) UpdateEditMode(false, false);
    }

    private void StopEditing()
    {
        if (EnableResizing) EnableResizing = false;
        if (EnableMoving) EnableMoving = false;
    }

    private unsafe void UpdateLockButton()
    {
        if (!EnableMoving || !TryGetLockButtonPosition(ScreenPosition, Size, Scale.X,
                (Vector2)AtkStage.Instance()->ScreenSize, out var position))
        {
            if (lockButton != null) lockButton.IsVisible = false;
            return;
        }

        if (lockButton == null)
        {
            lockButton = new TextButtonNode
            {
                String = "Lock",
                TextTooltip = "Lock position and size",
                Size = LockButtonSize,
                OnClick = () =>
                {
                    lockButton?.HideTooltip();
                    System.OverlayManager.RequestMeterLock(this);
                },
            };
            lockButton.AttachNode(this);
        }

        if (lockButton.Position != position) lockButton.Position = position;
        lockButton.IsVisible = true;
    }

    internal static bool TryGetLockButtonPosition(Vector2 origin, Vector2 size, float scale, Vector2 screen, out Vector2 position)
    {
        position = default;
        if (!float.IsFinite(scale) || scale <= 0) return false;

        const float clearance = EditMargin + LockButtonGap;
        var min = -origin / scale;
        var max = (screen - origin) / scale - LockButtonSize;
        if (min.X > max.X || min.Y > max.Y) return false;

        var x = Math.Clamp(size.X - LockButtonSize.X, min.X, max.X);
        var y = Math.Clamp(0, min.Y, max.Y);
        var above = -clearance - LockButtonSize.Y;
        var left = -clearance - LockButtonSize.X;
        if (above >= min.Y && above <= max.Y) position = new Vector2(x, above);
        else if (size.Y + clearance >= min.Y && size.Y + clearance <= max.Y) position = new Vector2(x, size.Y + clearance);
        else if (left >= min.X && left <= max.X) position = new Vector2(left, y);
        else if (size.X + clearance >= min.X && size.X + clearance <= max.X) position = new Vector2(size.X + clearance, y);
        else return false;
        return true;
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

    private void ResizeListNodeSafely(ListNode<CombatantRowData, MeterRowListItemNode> listNode, Vector2 desiredSize)
    {
        if (listNode.Size == desiredSize)
            return;

        var oldRowCount = GetListNodeRowCount(listNode.Height, listNode.ItemSpacing);
        var newRowCount = GetListNodeRowCount(desiredSize.Y, listNode.ItemSpacing);

        if (oldRowCount != newRowCount)
        {
            listNode.DetachNode();
            listNode.Size = desiredSize;
            listNode.AttachNode(bodyLayer);
            return;
        }

        listNode.Size = desiredSize;
    }

    private void UpdateListItemSpacingSafely(ListNode<CombatantRowData, MeterRowListItemNode> listNode, float itemSpacing)
    {
        listNode.DetachNode();
        listNode.ItemSpacing = itemSpacing;
        listNode.AttachNode(bodyLayer);
    }

    private int GetListNodeRowCount(float height, float itemSpacing)
        => MeterRowBuilder.GetVisibleRowCount(height, listRowHeight, itemSpacing);

    private void OnCombatDataUpdated()
    {
        if (isDisposing) return;
        Interlocked.Exchange(ref rowsDirty, 1);
    }

    private void RebuildList()
    {
        if (MeterSettings == null) return;

        Interlocked.Exchange(ref rowsDirty, 0);

        var hasCombat = System.ActiveMeterService.HasCombatData();
        var localSample = !hasCombat ? placement?.Sample : null;
        var combatants = hasCombat
            ? System.ActiveMeterService.GetCombatants().ToList()
            : localSample != null ? new List<Combatant>(localSample.Members)
                : FakeCombatantFactory.CreateFixedCombatants(MeterSettings.MaxCombatants);

        placementEncounter = localSample?.Encounter;
        var rows = MeterRowBuilder.Build(combatants, MeterSettings, !hasCombat || System.Config.General.PreviewEnabled,
            localSample != null ? placement!.GetSampleMaximum : null);
        cachedOptionsList = rows.Expanded;
        selfOptionsList = rows.Self;
        lastRowState = GetRowState();
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
        StopEditing();
        if (hookedService != null)
        {
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;
            hookedService = null;
        }
    }
}
