using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode
{
    private CheckboxRowNode showMemberBarsToggle = null!;
    private LabeledInputPairRowNode barOffsetRow = null!;
    private LabeledInputPairRowNode barSizeRow = null!;
    private LabeledNumericInputNode barOffsetXInput = null!;
    private LabeledNumericInputNode barOffsetYInput = null!;
    private LabeledNumericInputNode barWidthInput = null!;
    private LabeledNumericInputNode barHeightInput = null!;
    private LabeledEnumDropdownNode<ProgressBarType> barTypeDropdown = null!;
    private LabeledEnumDropdownNode<ProgressBarSizing> barSizingDropdown = null!;
    private LabeledDropdownNode barFillStatDropdown = null!;
    private CheckboxRowNode barFillRightToLeftToggle = null!;
    private LabeledEnumDropdownNode<ColorMode> barColorModeDropdown = null!;
    private LabeledEnumDropdownNode<ProgressBarColorTreatment> barColorTreatmentDropdown = null!;
    private ColorInputRow barColorInput = null!;
    private ColorInputRow barBackgroundColorInput = null!;

    private void BuildMemberBarControls()
    {
        showMemberBarsToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("PartyListShowBars"),
            IsChecked = config.ShowMemberBars,
            TextTooltip = Loc.Get("PartyListShowBarsHelp"),
            OnClick = isChecked =>
            {
                config.ShowMemberBars = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        barOffsetRow = CreatePairedInputRow(Loc.Get("PartyListPosition"));
        barSizeRow = CreatePairedInputRow(Loc.Get("PartyListSize"));
        barOffsetXInput = CreateCompactNumericInput(Loc.Get("PartyListPosX"), config.BarOffsetX, -500, 500, value => config.BarOffsetX = value);
        barOffsetYInput = CreateCompactNumericInput(Loc.Get("PartyListPosY"), config.BarOffsetY, -500, 500, value => config.BarOffsetY = value);
        barWidthInput = CreateCompactNumericInput(Loc.Get("PartyListWidth"), config.BarWidth, 1, 500, value => config.BarWidth = value);
        barHeightInput = CreateCompactNumericInput(Loc.Get("PartyListHeight"), config.BarHeight, 1, 100, value => config.BarHeight = value);
        barOffsetRow.AddInputPair(barOffsetXInput, barOffsetYInput);
        barSizeRow.AddInputPair(barWidthInput, barHeightInput);

        barTypeDropdown = new LabeledEnumDropdownNode<ProgressBarType>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListBarType"),
            Options = ProgressBarTypeOptions.Ordered(),
            SelectedOption = config.BarType,
            OnOptionSelected = value =>
            {
                config.BarType = value;
                SaveAndUpdate();
                if (!isLoading) ApplyEnabledState();
            },
        };

        barSizingDropdown = new LabeledEnumDropdownNode<ProgressBarSizing>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListBarSizing"),
            Options = Enum.GetValues<ProgressBarSizing>().ToList(),
            SelectedOption = config.BarSizing,
            OnOptionSelected = value =>
            {
                config.BarSizing = value;
                SaveAndUpdate();
            },
        };

        barFillStatDropdown = new LabeledDropdownNode
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListFillStat"),
            GetOptionLabel = Loc.StatLabel,
            Options = StatSelector.GetAvailableStatSelectors(),
            SelectedOption = config.BarFillStat,
            OnOptionSelected = value =>
            {
                config.BarFillStat = StatSelector.NormalizeStatSelector(value);
                SaveAndUpdate();
            },
        };

        barFillRightToLeftToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("PartyListFillLeft"),
            IsChecked = config.BarFillRightToLeft,
            OnClick = isChecked =>
            {
                config.BarFillRightToLeft = isChecked;
                SaveAndUpdate();
            },
        };

        barColorModeDropdown = new LabeledEnumDropdownNode<ColorMode>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListBarColorMode"),
            Options = Enum.GetValues<ColorMode>().ToList(),
            SelectedOption = config.BarColorMode,
            OnOptionSelected = value =>
            {
                config.BarColorMode = value;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        barColorTreatmentDropdown = new LabeledEnumDropdownNode<ProgressBarColorTreatment>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListBarTint"),
            Options = Enum.GetValues<ProgressBarColorTreatment>().ToList(),
            SelectedOption = config.BarColorTreatment,
            OnOptionSelected = value =>
            {
                config.BarColorTreatment = value;
                SaveAndUpdate();
            },
        };

        barColorInput = new ColorInputRow
        {
            Label = Loc.Get("PartyListBarColor"),
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().BarColor,
            CurrentColor = config.BarColor,
            OnColorConfirmed = color =>
            {
                config.BarColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.BarColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.BarColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        barBackgroundColorInput = new ColorInputRow
        {
            Label = Loc.Get("PartyListBarBackground"),
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().BarBackgroundColor,
            CurrentColor = config.BarBackgroundColor,
            OnColorConfirmed = color =>
            {
                config.BarBackgroundColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.BarBackgroundColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.BarBackgroundColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };
    }
}
