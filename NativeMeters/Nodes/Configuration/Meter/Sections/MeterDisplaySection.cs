using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public sealed class MeterDisplaySection : MeterConfigSection
{
    private LabeledDropdownNode? statDropdown;
    private LabeledNumericInputNode? maxRowsInput;
    private CheckboxNode? backgroundCheckbox;
    private ColorInputRow? backgroundColorInput;
    private CheckboxNode? headerToggle;
    private LabeledNumericInputNode? headerHeightInput;
    private CheckboxNode? footerToggle;
    private LabeledNumericInputNode? footerHeightInput;
    private LabeledNumericInputNode? rowHeightInput;
    private LabeledNumericInputNode? rowSpacingInput;
    private CheckboxNode? showLimitBreakToggle;
    private CheckboxNode? showNonPlayerToggle;

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        IsInitialized = true;

        statDropdown!.SelectedOption = Settings.StatToTrack;
        maxRowsInput!.Value = Settings.MaxCombatants;
        rowHeightInput!.Value = (int)Settings.RowHeight;
        rowSpacingInput!.Value = (int)Settings.RowSpacing;
        backgroundCheckbox!.IsChecked = Settings.ShowWindowBackground;
        backgroundColorInput!.CurrentColor = Settings.WindowColor;
        backgroundColorInput!.DefaultColor = new MeterSettings().WindowColor;
        headerHeightInput!.Value = (int)Settings.HeaderHeight;
        footerHeightInput!.Value = (int)Settings.FooterHeight;
        headerToggle!.IsChecked = Settings.HeaderEnabled;
        footerToggle!.IsChecked = Settings.FooterEnabled;
        showLimitBreakToggle!.IsChecked = Settings.ShowLimitBreak;
        showNonPlayerToggle!.IsChecked = Settings.ShowNonPlayerCombatants;

        RecalculateLayout();
    }

    private void Initialize()
    {
        AddTab();
        statDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Sort By: ",
            Options = StatSelector.GetAvailableStatSelectors(),
            OnOptionSelected = val => Settings.StatToTrack = val,
        };

        maxRowsInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Max Rows: ",
            Min = 1, Max = 40,
            OnValueUpdate = val => Settings.MaxCombatants = val,
        };

        rowHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Row Height:",
            Min = 10, Max = 100,
            OnValueUpdate = val =>
            {
                Settings.RowHeight = val;
                System.OverlayManager.Setup();
            },
        };

        rowSpacingInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Row Spacing:",
            Min = 0, Max = 100,
            OnValueUpdate = val => Settings.RowSpacing = val,
        };

        backgroundCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val => Settings.ShowWindowBackground = val,
        };

        backgroundColorInput = new ColorInputRow
        {
            Label = "Background Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new MeterSettings().WindowColor,
            CurrentColor = Settings.WindowColor,
            OnColorConfirmed = color => Settings.WindowColor = color,
            OnColorCanceled = color => Settings.WindowColor = color,
            OnColorPreviewed = color => Settings.WindowColor = color,
        };

        headerToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Header",
            OnClick = val => Settings.HeaderEnabled = val
        };

        headerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Header Space:",
            OnValueUpdate = val => Settings.HeaderHeight = val
        };

        footerToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Footer",
            OnClick = val => Settings.FooterEnabled = val
        };

        footerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Footer Space:",
            OnValueUpdate = val => Settings.FooterHeight = val
        };

        showLimitBreakToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Limit Break",
            OnClick = val => Settings.ShowLimitBreak = val
        };

        showNonPlayerToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Non Player Combatants",
            OnClick = val => Settings.ShowNonPlayerCombatants = val
        };


        AddNode([statDropdown, maxRowsInput, rowHeightInput, rowSpacingInput, backgroundCheckbox, backgroundColorInput, headerToggle, headerHeightInput, footerToggle, footerHeightInput, showLimitBreakToggle, showNonPlayerToggle]);
    }
}
