using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDefinitionConfigurationNode : SimpleComponentNode
{
    public Action? OnLayoutChanged { get; init; }

    private MeterSettings _settings = new();

    private readonly ScrollingAreaNode<TreeListNode> _scrollingArea;
    private readonly MeterGeneralSection _generalSettings;
    private readonly MeterDisplaySection _displaySettings;

    public MeterDefinitionConfigurationNode()
    {
        _scrollingArea = new ScrollingAreaNode<TreeListNode>
        {
            ContentHeight = 100.0f,
            AutoHideScrollBar = true,
        };
        _scrollingArea.AttachNode(this);

        _scrollingArea.ContentNode.OnLayoutUpdate = newHeight =>
        {
            _scrollingArea.ContentHeight = newHeight;
        };

        _scrollingArea.ContentNode.CategoryVerticalSpacing = 4.0f;

        var treeListNode = _scrollingArea.ContentAreaNode;

        _generalSettings = new MeterGeneralSection(() => _settings)
        {
            String = "General Settings",
            IsCollapsed = false,
        };
        _generalSettings.OnToggle = _ => HandleLayoutChange();
        treeListNode.AddCategoryNode(_generalSettings);

        _displaySettings = new MeterDisplaySection(() => _settings)
        {
            String = "Display Settings",
            IsCollapsed = false,
        };
        _displaySettings.OnToggle = _ => HandleLayoutChange();
        treeListNode.AddCategoryNode(_displaySettings);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _scrollingArea.Size = Size;

        foreach (var categoryNode in _scrollingArea.ContentNode.CategoryNodes)
        {
            categoryNode.Width = Width - 16.0f;
        }

        _scrollingArea.ContentNode.RefreshLayout();
    }

    public void SetMeter(MeterSettings settings)
    {
        _settings = settings;
        _generalSettings.Refresh();
        _displaySettings.Refresh();
        HandleLayoutChange();
    }

    private void HandleLayoutChange()
    {
        _scrollingArea.ContentNode.RefreshLayout();
        OnLayoutChanged?.Invoke();
    }
}

public abstract class MeterConfigSection : TreeListCategoryNode
{
    protected readonly Func<MeterSettings> GetMeterSettings;
    protected MeterSettings Settings => GetMeterSettings();

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;
        VerticalPadding = 4.0f;
    }

    public abstract void Refresh();

    protected static LabelTextNode CreateLabel(string text) => new()
    {
        TextFlags = TextFlags.AutoAdjustNodeSize,
        Size = new Vector2(100, 20),
        String = text,
    };
}

public sealed class MeterGeneralSection : MeterConfigSection
{
    private LabeledTextInputNode? nameInput;
    private CheckboxNode? enabledCheckbox;
    private CheckboxNode? lockedCheckbox;

    public MeterGeneralSection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (nameInput == null) Initialize();

        nameInput!.Text = Settings.Name;
        enabledCheckbox!.IsChecked = Settings.IsEnabled;
        lockedCheckbox!.IsChecked = Settings.IsLocked;

        RecalculateLayout();
    }

    private void Initialize()
    {
        nameInput = new LabeledTextInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Meter Name: ",
            OnInputComplete = val => Settings.Name = val.ToString(),
        };
        AddNode(nameInput);

        enabledCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enabled",
            OnClick = val => { Settings.IsEnabled = val; System.OverlayManager.Setup(); },
        };
        AddNode(enabledCheckbox);

        lockedCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Lock Position/Size",
            OnClick = val => Settings.IsLocked = val,
        };
        AddNode(lockedCheckbox);
    }
}

public sealed class MeterDisplaySection : MeterConfigSection
{
    private LabeledDropdownNode? statDropdown;
    private LabeledDropdownNode? iconTypeDropdown;
    private LabeledDropdownNode? progressBarTypeDropdown;
    private LabeledNumericInputNode? maxRowsInput;
    private CheckboxNode? backgroundCheckbox;

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        statDropdown!.SelectedOption = Settings.StatToTrack;
        iconTypeDropdown!.SelectedOption = Settings.JobIconType.ToString();
        progressBarTypeDropdown!.SelectedOption = Settings.ProgressBarType.ToString();
        maxRowsInput!.Value = Settings.MaxCombatants;
        backgroundCheckbox!.IsChecked = Settings.BackgroundEnabled;

        RecalculateLayout();
    }

    private void Initialize()
    {
        statDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Stat Type: ",
            Options = new List<string> { "ENCDPS", "ENCHPS", "Damage%", "DPS" },
            OnOptionSelected = val => Settings.StatToTrack = val,
        };
        AddNode(statDropdown);

        maxRowsInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Max Rows: ",
            Min = 1, Max = 40,
            OnValueUpdate = val => Settings.MaxCombatants = val,
        };
        AddNode(maxRowsInput);

        iconTypeDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Job Icon Type: ",
            Options = Enum.GetNames(typeof(JobIconType)).ToList(),
            OnOptionSelected = selected =>
            {
                if (Enum.TryParse<JobIconType>(selected, out var parsed))
                {
                    Settings.JobIconType = parsed;
                }
            }
        };
        AddNode(iconTypeDropdown);

        progressBarTypeDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Progress Bar Type: ",
            Options = Enum.GetNames(typeof(ProgressBarType)).ToList(),
            OnOptionSelected = selected =>
            {
                if (Enum.TryParse<ProgressBarType>(selected, out var parsed))
                {
                    Settings.ProgressBarType = parsed;
                }
            }
        };
        AddNode(progressBarTypeDropdown);

        backgroundCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val => Settings.BackgroundEnabled = val,
        };
        AddNode(backgroundCheckbox);
    }
}