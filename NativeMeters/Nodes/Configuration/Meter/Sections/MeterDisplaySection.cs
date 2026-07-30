using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
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
    private LabeledNumericInputNode? scaleInput;
    private CheckboxRowNode? backgroundCheckbox;
    private ColorInputRow? backgroundColorInput;
    private CheckboxRowNode? headerToggle;
    private LabeledNumericInputNode? headerHeightInput;
    private CheckboxRowNode? footerToggle;
    private LabeledNumericInputNode? footerHeightInput;
    private LabeledNumericInputNode? rowHeightInput;
    private LabeledNumericInputNode? rowSpacingInput;
    private CheckboxRowNode? showLimitBreakToggle;
    private CheckboxRowNode? showNonPlayerToggle;
    private CheckboxRowNode? showPinSelfToggle;
    private CheckboxRowNode? selfHighlightToggle;
    private LabeledEnumDropdownNode<SelfTextOverrideMode>? selfTextTargetDropdown;
    private CheckboxRowNode? selfTextColorToggle;
    private ColorInputRow? selfTextColorInput;
    private CheckboxRowNode? selfTextOutlineColorToggle;
    private ColorInputRow? selfTextOutlineColorInput;
    private CheckboxRowNode? selfBarColorToggle;
    private ColorInputRow? selfBarColorInput;
    private CheckboxRowNode? selfTextStyleToggle;
    private LabeledNumericInputNode? selfTextFontSizeInput;
    private LabeledEnumDropdownNode<FontType>? selfTextFontTypeDropdown;
    private LabeledTextFlagsInputNode? selfTextFlagsInput;
    private bool isLoading;

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    private SelfRowOverrideSettings SelfRowOverride
    {
        get
        {
            Settings.EnsureInitialized();
            return Settings.SelfRowOverride;
        }
    }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        Settings.EnsureInitialized();
        IsInitialized = true;

        isLoading = true;
        try
        {
            statDropdown!.SelectedOption = Settings.StatToTrack;
            maxRowsInput!.Value = Settings.MaxCombatants;
            scaleInput!.Value = Settings.Scale;
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
            showPinSelfToggle!.IsChecked = Settings.PinSelfToTop;
            selfHighlightToggle!.IsChecked = SelfRowOverride.Enabled;
            selfTextTargetDropdown!.SelectedOption = SelfRowOverride.TextColorMode;
            selfTextColorToggle!.IsChecked = SelfRowOverride.OverrideTextColor;
            selfTextColorInput!.CurrentColor = SelfRowOverride.TextColor;
            selfTextColorInput!.DefaultColor = new SelfRowOverrideSettings().TextColor;
            selfTextOutlineColorToggle!.IsChecked = SelfRowOverride.OverrideTextOutlineColor;
            selfTextOutlineColorInput!.CurrentColor = SelfRowOverride.TextOutlineColor;
            selfTextOutlineColorInput!.DefaultColor = new SelfRowOverrideSettings().TextOutlineColor;
            selfBarColorToggle!.IsChecked = SelfRowOverride.OverrideBarColor;
            selfBarColorInput!.CurrentColor = SelfRowOverride.BarColor;
            selfBarColorInput!.DefaultColor = new SelfRowOverrideSettings().BarColor;
            selfTextStyleToggle!.IsChecked = SelfRowOverride.OverrideTextStyle;
            selfTextFontSizeInput!.Value = (int)SelfRowOverride.TextFontSize;
            selfTextFontTypeDropdown!.SelectedOption = SelfRowOverride.TextFontType;
            selfTextFlagsInput!.Value = SelfRowOverride.TextFlags;
        }
        finally
        {
            isLoading = false;
        }

        ApplyDisplayDependencyState();
        ApplySelfOverrideState();
        RecalculateSectionLayout();
    }

    private void Initialize()
    {
        statDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Sort By: ",
            Options = StatSelector.GetAvailableStatSelectors(),
            OnOptionSelected = val =>
            {
                if (isLoading) return;
                Settings.StatToTrack = val;
            },
        };

        maxRowsInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Max Rows: ",
            Min = 1, Max = 40,
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.MaxCombatants = val;
            },
        };

        scaleInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Scale %:",
            Min = 10, Max = 400,
            Step = 10,
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.Scale = val;
            },
        };

        rowHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Row Height:",
            Min = 10, Max = 100,
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.RowHeight = val;
                System.OverlayManager.Setup();
            },
        };

        rowSpacingInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Row Spacing:",
            Min = 0, Max = 100,
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.RowSpacing = val;
            },
        };

        backgroundCheckbox = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.ShowWindowBackground = val;
                ApplyDisplayDependencyState();
            },
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

        headerToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Header",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.HeaderEnabled = val;
                ApplyDisplayDependencyState();
            }
        };

        headerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Header Space:",
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.HeaderHeight = val;
            }
        };

        footerToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Footer",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.FooterEnabled = val;
                ApplyDisplayDependencyState();
            }
        };

        footerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Footer Space:",
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                Settings.FooterHeight = val;
            }
        };

        showLimitBreakToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Limit Break",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.ShowLimitBreak = val;
            }
        };

        showNonPlayerToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Non Player Combatants",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.ShowNonPlayerCombatants = val;
            }
        };

        showPinSelfToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Pin Self To Top",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.PinSelfToTop = val;
                System.OverlayManager.Setup();
            }
        };

        selfHighlightToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Highlight Self",
            TextTooltip = "Apply meter-specific colors to your own row.",
            OnClick = val =>
            {
                if (isLoading) return;
                SelfRowOverride.Enabled = val;
                ApplySelfOverrideState();
                RecalculateSectionLayout();
            }
        };

        selfTextTargetDropdown = new LabeledEnumDropdownNode<SelfTextOverrideMode>
        {
            Size = new Vector2(Width, 28),
            LabelText = "Self Text Target:",
            Options = Enum.GetValues<SelfTextOverrideMode>().ToList(),
            SelectedOption = SelfRowOverride.TextColorMode,
            OnOptionSelected = val =>
            {
                if (isLoading) return;
                SelfRowOverride.TextColorMode = val;
            },
        };

        selfTextColorToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Override Self Text Color",
            TextTooltip = "Uses the Self Text Target setting above.",
            OnClick = val =>
            {
                if (isLoading) return;
                SelfRowOverride.OverrideTextColor = val;
                ApplySelfOverrideState();
                RecalculateSectionLayout();
            }
        };

        selfTextColorInput = new ColorInputRow
        {
            Label = "Self Text Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new SelfRowOverrideSettings().TextColor,
            CurrentColor = SelfRowOverride.TextColor,
            OnColorConfirmed = color => SelfRowOverride.TextColor = color,
            OnColorCanceled = color => SelfRowOverride.TextColor = color,
            OnColorPreviewed = color => SelfRowOverride.TextColor = color,
        };

        selfTextOutlineColorToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Override Self Outline Color",
            TextTooltip = "Uses the Self Text Target setting above.",
            OnClick = val =>
            {
                if (isLoading) return;
                SelfRowOverride.OverrideTextOutlineColor = val;
                ApplySelfOverrideState();
                RecalculateSectionLayout();
            }
        };

        selfTextOutlineColorInput = new ColorInputRow
        {
            Label = "Self Outline Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new SelfRowOverrideSettings().TextOutlineColor,
            CurrentColor = SelfRowOverride.TextOutlineColor,
            OnColorConfirmed = color => SelfRowOverride.TextOutlineColor = color,
            OnColorCanceled = color => SelfRowOverride.TextOutlineColor = color,
            OnColorPreviewed = color => SelfRowOverride.TextOutlineColor = color,
        };

        selfBarColorToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Override Self Bar Color",
            OnClick = val =>
            {
                if (isLoading) return;
                SelfRowOverride.OverrideBarColor = val;
                ApplySelfOverrideState();
                RecalculateSectionLayout();
            }
        };

        selfBarColorInput = new ColorInputRow
        {
            Label = "Self Bar Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new SelfRowOverrideSettings().BarColor,
            CurrentColor = SelfRowOverride.BarColor,
            OnColorConfirmed = color => SelfRowOverride.BarColor = color,
            OnColorCanceled = color => SelfRowOverride.BarColor = color,
            OnColorPreviewed = color => SelfRowOverride.BarColor = color,
        };

        selfTextStyleToggle = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Override Self Text Style",
            TextTooltip = "Uses the Self Text Target setting above.",
            OnClick = val =>
            {
                if (isLoading) return;
                SelfRowOverride.OverrideTextStyle = val;
                ApplySelfOverrideState();
                RecalculateSectionLayout();
            }
        };

        selfTextFontSizeInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Self Text Size:",
            Min = 6,
            Max = 72,
            OnValueUpdate = val =>
            {
                if (isLoading) return;
                SelfRowOverride.TextFontSize = (uint)val;
            },
        };

        selfTextFontTypeDropdown = new LabeledEnumDropdownNode<FontType>
        {
            Size = new Vector2(Width, 28),
            LabelText = "Self Text Font:",
            Options = Enum.GetValues<FontType>().ToList(),
            SelectedOption = SelfRowOverride.TextFontType,
            OnOptionSelected = val =>
            {
                if (isLoading) return;
                SelfRowOverride.TextFontType = val;
            },
        };

        selfTextFlagsInput = new LabeledTextFlagsInputNode
        {
            Width = Width,
            LabelText = "Self Text Style:",
            OnValueChanged = val =>
            {
                if (isLoading) return;
                SelfRowOverride.TextFlags = val;
            },
        };

        AddNode(statDropdown);
        AddNode(maxRowsInput);
        AddNode(scaleInput);
        AddNode(rowHeightInput);
        AddNode(rowSpacingInput);
        AddNode(backgroundCheckbox);
        AddTab(1);
        AddNode(backgroundColorInput);
        SubtractTab(1);
        AddNode(headerToggle);
        AddTab(1);
        AddNode(headerHeightInput);
        SubtractTab(1);
        AddNode(footerToggle);
        AddTab(1);
        AddNode(footerHeightInput);
        SubtractTab(1);
        AddNode(showLimitBreakToggle);
        AddNode(showNonPlayerToggle);
        AddNode(showPinSelfToggle);
        AddNode(selfHighlightToggle);
        AddTab(1);
        AddNode(selfTextTargetDropdown);
        AddNode(selfTextColorToggle);
        AddTab(1);
        AddNode(selfTextColorInput);
        SubtractTab(1);
        AddNode(selfTextOutlineColorToggle);
        AddTab(1);
        AddNode(selfTextOutlineColorInput);
        SubtractTab(1);
        AddNode(selfBarColorToggle);
        AddTab(1);
        AddNode(selfBarColorInput);
        SubtractTab(1);
        AddNode(selfTextStyleToggle);
        AddTab(1);
        AddNode(selfTextFontSizeInput);
        AddNode(selfTextFontTypeDropdown);
        AddNode(selfTextFlagsInput);
        SubtractTab(1);
        SubtractTab(1);

        ApplySelfOverrideState();
    }

    private void ApplyDisplayDependencyState()
    {
        if (backgroundColorInput == null
            || headerHeightInput == null
            || footerHeightInput == null)
        {
            return;
        }

        backgroundColorInput.IsEnabled = Settings.ShowWindowBackground;
        headerHeightInput.IsEnabled = Settings.HeaderEnabled;
        footerHeightInput.IsEnabled = Settings.FooterEnabled;
    }

    private void ApplySelfOverrideState()
    {
        if (selfTextTargetDropdown == null
            || selfTextColorToggle == null
            || selfTextColorInput == null
            || selfTextOutlineColorToggle == null
            || selfTextOutlineColorInput == null
            || selfBarColorToggle == null
            || selfBarColorInput == null
            || selfTextStyleToggle == null
            || selfTextFontSizeInput == null
            || selfTextFontTypeDropdown == null
            || selfTextFlagsInput == null)
        {
            return;
        }

        var isEnabled = SelfRowOverride.Enabled;
        var isTextColorEnabled = isEnabled && SelfRowOverride.OverrideTextColor;
        var isTextOutlineColorEnabled = isEnabled && SelfRowOverride.OverrideTextOutlineColor;
        var isBarColorEnabled = isEnabled && SelfRowOverride.OverrideBarColor;
        var isTextStyleEnabled = isEnabled && SelfRowOverride.OverrideTextStyle;

        selfTextTargetDropdown.IsVisible = true;
        selfTextColorToggle.IsVisible = true;
        selfTextColorInput.IsVisible = true;
        selfTextOutlineColorToggle.IsVisible = true;
        selfTextOutlineColorInput.IsVisible = true;
        selfBarColorToggle.IsVisible = true;
        selfBarColorInput.IsVisible = true;
        selfTextStyleToggle.IsVisible = true;
        selfTextFontSizeInput.IsVisible = true;
        selfTextFontTypeDropdown.IsVisible = true;
        selfTextFlagsInput.IsVisible = true;

        selfTextTargetDropdown.IsEnabled = isEnabled;
        SetCheckboxEnabled(selfTextColorToggle, isEnabled);
        selfTextColorInput.IsEnabled = isTextColorEnabled;
        SetCheckboxEnabled(selfTextOutlineColorToggle, isEnabled);
        selfTextOutlineColorInput.IsEnabled = isTextOutlineColorEnabled;
        SetCheckboxEnabled(selfBarColorToggle, isEnabled);
        selfBarColorInput.IsEnabled = isBarColorEnabled;
        SetCheckboxEnabled(selfTextStyleToggle, isEnabled);
        selfTextFontSizeInput.IsEnabled = isTextStyleEnabled;
        selfTextFontTypeDropdown.IsEnabled = isTextStyleEnabled;
        selfTextFlagsInput.IsEnabled = isTextStyleEnabled;
    }

    private static void SetCheckboxEnabled(CheckboxRowNode checkbox, bool isEnabled)
        => checkbox.IsEnabled = isEnabled;
}
