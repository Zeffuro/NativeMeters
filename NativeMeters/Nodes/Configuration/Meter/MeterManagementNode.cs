using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using Lumina.Data.Parsing.Uld;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Services;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterManagementNode : SimpleComponentNode
{
    private const float MinimumListWidth = 180.0f;
    private const float MaximumListWidth = 240.0f;
    private const float SeparatorWidth = 4.0f;
    private const float LayoutSpacing = 4.0f;
    private const float SearchHeight = 28.0f;
    private const float ButtonHeight = 26.0f;
    private const float RowSpacing = 8.0f;

    private readonly HorizontalListNode layoutContainer;
    private readonly VerticalListNode selectionColumn;
    private readonly TextInputNode searchInputNode;
    private readonly ListNode<MeterSettings, MeterListItemNode> selectionListNode;
    private readonly HorizontalFlexNode buttonRow;
    private readonly TextButtonNode addButtonNode;
    private readonly TextButtonNode removeButtonNode;
    private readonly VerticalLineNode separatorNode;
    private readonly MeterConfigurationNode configNode;
    private readonly AddMeterDialogAddon addMeterDialog;
    private readonly List<MeterSettings> meterSettings;

    private MeterSettings? selectedMeter;
    private string currentSearch = string.Empty;
    private readonly bool ownsAddMeterDialog;

    public int TabBarNavIndex
    {
        get;
        set
        {
            field = value;
            ApplyNavigation();
        }
    } = 3;

    public MeterManagementNode(AddMeterDialogAddon? sharedAddMeterDialog = null)
    {
        meterSettings = System.Config.Meters.ToList();
        SortMeters();

        ownsAddMeterDialog = sharedAddMeterDialog is null;
        addMeterDialog = sharedAddMeterDialog ?? new AddMeterDialogAddon
        {
            InternalName = "NativeMetersAddMeter",
            Title = "Add Meter",
            Size = new Vector2(480.0f, 300.0f),
            RememberClosePosition = false,
        };
        addMeterDialog.OnMeterCreated = AddMeter;

        searchInputNode = new TextInputNode
        {
            Height = SearchHeight,
            PlaceholderStringId = 325,
            SheetType = NodeData.SheetType.Addon,
            OnInputReceived = OnSearchInputReceived,
            NavIndex = 6,
            NavDown = 7,
            NavUp = TabBarNavIndex,
            NavRight = 150,
        };

        selectionListNode = new ListNode<MeterSettings, MeterListItemNode>
        {
            AllowMultipleSelection = false,
            ItemSpacing = 2.0f,
            OptionsList = meterSettings,
            OnItemSelected = SelectMeter,
            NoResultsString = "No meters found.",
            NavIndex = 7,
            NavUp = 6,
            NavDown = 100,
            NavRight = 150,
        };

        buttonRow = new HorizontalFlexNode
        {
            Height = ButtonHeight,
            AlignmentFlags = FlexFlags.FitHeight | FlexFlags.FitWidth,
            ItemSpacing = 6.0f,
            InitialNodes =
            [
                addButtonNode = new TextButtonNode
                {
                    TextId = 302, // Add
                    SheetType = NodeData.SheetType.Addon,
                    OnClick = () => addMeterDialog.Open(),
                    NavIndex = 100,
                    NavUp = 7,
                    NavDown = TabBarNavIndex,
                    NavRight = 101,
                },
                removeButtonNode = new TextButtonNode
                {
                    TextId = 85, // Remove
                    SheetType = NodeData.SheetType.Addon,
                    OnClick = OnRemoveSelectedMeter,
                    IsEnabled = false,
                    NavIndex = 101,
                    NavUp = 7,
                    NavDown = TabBarNavIndex,
                    NavLeft = 100,
                    NavRight = 150,
                },
            ],
        };

        selectionColumn = new VerticalListNode
        {
            FitWidth = true,
            InitialNodes =
            [
                searchInputNode,
                new ResNode { Height = RowSpacing },
                selectionListNode,
                new ResNode { Height = RowSpacing },
                buttonRow,
            ],
        };

        separatorNode = new VerticalLineNode { Width = SeparatorWidth };
        configNode = new MeterConfigurationNode();

        layoutContainer = new HorizontalListNode
        {
            ReverseLayoutUpdate = true,
            FitHeight = true,
            ItemSpacing = LayoutSpacing,
            InitialNodes =
            [
                selectionColumn,
                separatorNode,
                configNode,
            ],
        };
        layoutContainer.AttachNode(this);

        ApplyNavigation();
    }

    public void ApplyResolvedText()
    {
        var searchPlaceholder = searchInputNode.PlaceholderTextNode.String.ToString();
        searchInputNode.PlaceholderString = string.IsNullOrEmpty(searchPlaceholder) ? "Search" : searchPlaceholder;
    }

    public void RecalculateLayout()
        => OnSizeChanged();

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        layoutContainer.Size = Size;

        var maxListWidth = Math.Max(0.0f, Width - SeparatorWidth - LayoutSpacing * 2.0f);
        var listWidth = Math.Min(MaximumListWidth, Math.Max(MinimumListWidth, Width * 0.35f));
        listWidth = Math.Min(listWidth, maxListWidth);

        selectionColumn.Size = new Vector2(listWidth, Height);
        searchInputNode.Width = listWidth;
        buttonRow.Width = listWidth;

        selectionListNode.Size = new Vector2(listWidth, Math.Max(0.0f, Height - SearchHeight - ButtonHeight - RowSpacing * 2.0f));

        separatorNode.Height = Height;

        var configX = listWidth + SeparatorWidth + LayoutSpacing * 2.0f;
        configNode.Size = new Vector2(Math.Max(0.0f, Width - configX), Height);

        layoutContainer.RecalculateLayout();
        ApplyNavigation();
    }

    private void OnSearchInputReceived(ReadOnlySeString searchString)
    {
        currentSearch = searchString.ToString();
        RefreshMeterList(clearSelection: true);
    }

    private void SelectMeter(MeterSettings? meter)
    {
        selectedMeter = meter;
        removeButtonNode.IsEnabled = meter is not null;
        configNode.SelectEntry(meter);

        selectionListNode.ClearSelection();
        if (meter is not null && selectionListNode.OptionsList.Contains(meter))
        {
            selectionListNode.SelectedItems.Add(meter);
            selectionListNode.Update();
        }
    }

    private void AddMeter(MeterSettings newMeter)
    {
        if (string.IsNullOrWhiteSpace(newMeter.Name))
        {
            newMeter.Name = $"New Meter {System.Config.Meters.Count + 1}";
        }

        newMeter.IsLocked = false;
        ClampMeterToScreen(newMeter);

        System.Config.Meters.Add(newMeter);
        ConfigRepository.Save(System.Config);

        meterSettings.Add(newMeter);
        SortMeters();

        currentSearch = string.Empty;
        searchInputNode.String = string.Empty;
        RefreshMeterList(clearSelection: false);
        SelectMeter(newMeter);

        System.OverlayManager.Setup();
    }

    private void OnRemoveSelectedMeter()
    {
        if (selectedMeter is not null)
        {
            OnRemoveMeter(selectedMeter);
        }
    }

    private void OnRemoveMeter(MeterSettings meter)
    {
        System.Config.Meters.Remove(meter);
        ConfigRepository.Save(System.Config);
        meterSettings.Remove(meter);

        if (ReferenceEquals(selectedMeter, meter))
        {
            SelectMeter(null);
        }

        RefreshMeterList(clearSelection: false);
        System.OverlayManager.Setup();
    }

    private void RefreshMeterList(bool clearSelection)
    {
        var filteredMeters = string.IsNullOrWhiteSpace(currentSearch)
            ? meterSettings.ToList()
            : meterSettings
                .Where(settings => MeterListItemNode.GetLabel(settings).Contains(currentSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();

        selectionListNode.OptionsList = filteredMeters;
        selectionListNode.ResetScroll();

        if (clearSelection || selectedMeter is null || !filteredMeters.Contains(selectedMeter))
        {
            SelectMeter(null);
        }
        else
        {
            SelectMeter(selectedMeter);
        }
    }

    private void SortMeters()
    {
        meterSettings.Sort(MeterListItemNode.Compare);
    }

    private void ApplyNavigation()
    {
        if (searchInputNode is null || selectionListNode is null || addButtonNode is null || removeButtonNode is null)
        {
            return;
        }

        searchInputNode.NavIndex = 6;
        searchInputNode.NavUp = TabBarNavIndex;
        searchInputNode.NavDown = 7;
        searchInputNode.NavRight = 150;

        selectionListNode.NavIndex = 7;
        selectionListNode.NavUp = 6;
        selectionListNode.NavDown = 100;
        selectionListNode.NavRight = 150;

        addButtonNode.NavIndex = 100;
        addButtonNode.NavUp = 7;
        addButtonNode.NavDown = TabBarNavIndex;
        addButtonNode.NavRight = 101;

        removeButtonNode.NavIndex = 101;
        removeButtonNode.NavUp = 7;
        removeButtonNode.NavDown = TabBarNavIndex;
        removeButtonNode.NavLeft = 100;
        removeButtonNode.NavRight = 150;
    }

    private static unsafe void ClampMeterToScreen(MeterSettings meter)
    {
        var screenSize = (Vector2)AtkStage.Instance()->ScreenSize;
        var scale = Math.Max(0.1f, meter.Scale / 100.0f);
        var scaledSize = meter.Size * scale;

        var maxX = Math.Max(0.0f, screenSize.X - scaledSize.X);
        var maxY = Math.Max(0.0f, screenSize.Y - scaledSize.Y);

        meter.Position = new Vector2(
            Math.Clamp(meter.Position.X, 0.0f, maxX),
            Math.Clamp(meter.Position.Y, 0.0f, maxY));
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        if (disposing && !isNativeDestructor && ownsAddMeterDialog)
        {
            addMeterDialog.Dispose();
        }

        base.Dispose(disposing, isNativeDestructor);
    }
}
