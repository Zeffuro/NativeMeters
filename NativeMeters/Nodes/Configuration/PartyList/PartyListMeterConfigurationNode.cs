using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Data.Stats;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode : TabbedVerticalListNode
{
    private const float CategoryHeight = 16.0f;
    private const float CheckboxHeight = 28.0f;
    private const float ControlHeight = 28.0f;
    private const float FormatButtonSize = 24.0f;
    private const float ResetButtonHeight = 28.0f;

    private readonly PartyListMeterSettings config = System.Config.PartyListMeter;

    private CheckboxRowNode enabledToggle = null!;
    private HoldButtonNode resetButton = null!;
    private HorizontalListNode formatRow = null!;
    private LabeledTextInputNode formatInput = null!;
    private CircleButtonNode formatHelpButton = null!;
    private CircleButtonNode browseTagButton = null!;
    private CheckboxRowNode highlightTopMemberToggle = null!;
    private LabeledDropdownNode topMemberStatDropdown = null!;
    private HorizontalListNode topMemberFormatRow = null!;
    private LabeledTextInputNode topMemberFormatInput = null!;
    private CircleButtonNode topMemberFormatHelpButton = null!;
    private CircleButtonNode topMemberBrowseTagButton = null!;
    private ColorInputRow topMemberTextColorInput = null!;
    private LabeledInputPairRowNode memberOffsetRow = null!;
    private LabeledInputPairRowNode memberSizeRow = null!;
    private LabeledNumericInputNode offsetXInput = null!;
    private LabeledNumericInputNode offsetYInput = null!;
    private LabeledNumericInputNode widthInput = null!;
    private LabeledNumericInputNode heightInput = null!;
    private LabeledNumericInputNode fontSizeInput = null!;
    private LabeledEnumDropdownNode<FontType> fontTypeDropdown = null!;
    private LabeledTextFlagsInputNode textFlagsInput = null!;
    private LabeledEnumDropdownNode<AlignmentType> memberAlignmentDropdown = null!;
    private ColorInputRow textColorInput = null!;
    private ColorInputRow outlineColorInput = null!;

    private CheckboxRowNode showSelfToggle = null!;
    private CheckboxRowNode showPartyToggle = null!;
    private CheckboxRowNode hideWhenNoDataToggle = null!;
    private LabeledEnumDropdownNode<PartyListMeterAnchor> anchorDropdown = null!;
    private CheckboxRowNode showMemberBarsToggle = null!;
    private LabeledInputPairRowNode barOffsetRow = null!;
    private LabeledInputPairRowNode barSizeRow = null!;
    private LabeledNumericInputNode barOffsetXInput = null!;
    private LabeledNumericInputNode barOffsetYInput = null!;
    private LabeledNumericInputNode barWidthInput = null!;
    private LabeledNumericInputNode barHeightInput = null!;
    private LabeledEnumDropdownNode<ProgressBarType> barTypeDropdown = null!;
    private CheckboxRowNode barFillRightToLeftToggle = null!;
    private LabeledEnumDropdownNode<ColorMode> barColorModeDropdown = null!;
    private LabeledEnumDropdownNode<ProgressBarColorTreatment> barColorTreatmentDropdown = null!;
    private ColorInputRow barColorInput = null!;
    private ColorInputRow barBackgroundColorInput = null!;
    private CheckboxRowNode showRaidDpsToggle = null!;
    private HorizontalListNode raidFormatRow = null!;
    private LabeledTextInputNode raidFormatInput = null!;
    private CircleButtonNode raidFormatHelpButton = null!;
    private CircleButtonNode raidBrowseTagButton = null!;
    private LabeledInputPairRowNode raidOffsetRow = null!;
    private LabeledInputPairRowNode raidSizeRow = null!;
    private LabeledNumericInputNode raidOffsetXInput = null!;
    private LabeledNumericInputNode raidOffsetYInput = null!;
    private LabeledNumericInputNode raidWidthInput = null!;
    private LabeledNumericInputNode raidHeightInput = null!;
    private LabeledEnumDropdownNode<AlignmentType> raidAlignmentDropdown = null!;
    private LabeledNumericInputNode raidFontSizeInput = null!;
    private LabeledEnumDropdownNode<FontType> raidFontTypeDropdown = null!;
    private LabeledTextFlagsInputNode raidTextFlagsInput = null!;
    private ColorInputRow raidTextColorInput = null!;
    private ColorInputRow raidOutlineColorInput = null!;

    private bool isDisposed;
    private bool isLoading;
    private Action<string>? currentTagInsertAction;
    private Action<TagInfo>? currentTagSelectionAction;

    public PartyListMeterConfigurationNode()
    {
        ItemSpacing = 1;
        FitWidth = true;

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Party List Meter",
        });

        enabledToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Enabled",
            IsChecked = config.Enabled,
            TextTooltip = "Shows lightweight NativeMeters text on each party-list row.",
            OnClick = isChecked =>
            {
                config.Enabled = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        resetButton = new HoldButtonNode
        {
            Size = new Vector2(92, ResetButtonHeight),
            IsVisible = true,
            String = "Reset",
            TextNode = { TextColor = ColorHelper.GetColor(50) },
            UnlockAfterClick = true,
            TextTooltip = "Reset Party List settings\n(hold button to confirm)",
            OnClick = ResetPartyListSettings,
        };

        showSelfToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Show Self",
            IsChecked = config.ShowSelf,
            OnClick = isChecked =>
            {
                config.ShowSelf = isChecked;
                SaveAndUpdate();
            },
        };

        showPartyToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Show Party Members",
            IsChecked = config.ShowPartyMembers,
            OnClick = isChecked =>
            {
                config.ShowPartyMembers = isChecked;
                SaveAndUpdate();
            },
        };

        hideWhenNoDataToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Hide Without Combat Data",
            IsChecked = config.HideWhenNoCombatData,
            OnClick = isChecked =>
            {
                config.HideWhenNoCombatData = isChecked;
                SaveAndUpdate();
            },
        };

        formatRow = new HorizontalListNode
        {
            Size = new Vector2(420, ControlHeight),
            ItemSpacing = 2.0f,
        };

        formatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Format:",
            Text = config.MemberFormat,
            Placeholder = "[dps:c.1]",
            OnInputComplete = value =>
            {
                config.MemberFormat = value.ToString();
                SaveAndUpdate();
            },
        };

        formatHelpButton = CreateFormatHelpButton();
        browseTagButton = CreateTagBrowserButton(() => OpenTagPicker(formatInput));
        formatRow.AddNode(new NodeBase[] { formatInput, formatHelpButton, browseTagButton });

        highlightTopMemberToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Highlight Top Member",
            IsChecked = config.HighlightTopMember,
            TextTooltip = "Applies a separate member text format and color to the highest party member for the selected stat.",
            OnClick = isChecked =>
            {
                config.HighlightTopMember = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        topMemberStatDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Top Stat:",
            Options = StatSelector.GetAvailableStatSelectors(),
            SelectedOption = config.TopMemberStat,
            OnOptionSelected = value =>
            {
                config.TopMemberStat = StatSelector.NormalizeStatSelector(value);
                SaveAndUpdate();
            },
        };

        topMemberFormatRow = new HorizontalListNode
        {
            Size = new Vector2(420, ControlHeight),
            ItemSpacing = 2.0f,
        };

        topMemberFormatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Top Format:",
            Text = config.TopMemberFormat,
            Placeholder = "[dps:c.1]",
            OnInputComplete = value =>
            {
                config.TopMemberFormat = value.ToString();
                SaveAndUpdate();
            },
        };

        topMemberFormatHelpButton = CreateFormatHelpButton();
        topMemberBrowseTagButton = CreateTagBrowserButton(() => OpenTagPicker(topMemberFormatInput));
        topMemberFormatRow.AddNode(new NodeBase[] { topMemberFormatInput, topMemberFormatHelpButton, topMemberBrowseTagButton });

        topMemberTextColorInput = new ColorInputRow
        {
            Label = "Top Text Color: ",
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().TopMemberTextColor,
            CurrentColor = config.TopMemberTextColor,
            OnColorConfirmed = color =>
            {
                config.TopMemberTextColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.TopMemberTextColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.TopMemberTextColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        anchorDropdown = new LabeledEnumDropdownNode<PartyListMeterAnchor>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Anchor:",
            Options = Enum.GetValues<PartyListMeterAnchor>().ToList(),
            SelectedOption = config.Anchor,
            OnOptionSelected = value =>
            {
                config.Anchor = value;
                SaveAndUpdate();
            },
        };

        memberOffsetRow = CreatePairedInputRow("Position:");
        memberSizeRow = CreatePairedInputRow("Size:");
        offsetXInput = CreateCompactNumericInput("X:", config.OffsetX, -500, 500, value => config.OffsetX = value);
        offsetYInput = CreateCompactNumericInput("Y:", config.OffsetY, -500, 500, value => config.OffsetY = value);
        widthInput = CreateCompactNumericInput("Width:", config.Width, 1, 500, value => config.Width = value);
        heightInput = CreateCompactNumericInput("Height:", config.Height, 1, 100, value => config.Height = value);
        fontSizeInput = CreateNumericInput("Font Size:", (int)config.FontSize, 6, 72, value => config.FontSize = (uint)value);
        memberOffsetRow.AddInputPair(offsetXInput, offsetYInput);
        memberSizeRow.AddInputPair(widthInput, heightInput);

        memberAlignmentDropdown = new LabeledEnumDropdownNode<AlignmentType>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Alignment:",
            Options = Enum.GetValues<AlignmentType>().ToList(),
            SelectedOption = config.MemberAlignment,
            OnOptionSelected = value =>
            {
                config.MemberAlignment = value;
                SaveAndUpdate();
            },
        };

        showMemberBarsToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Show Member Bars",
            IsChecked = config.ShowMemberBars,
            TextTooltip = "Shows a native-style DPS bar on each party-list row.",
            OnClick = isChecked =>
            {
                config.ShowMemberBars = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        barOffsetRow = CreatePairedInputRow("Position:");
        barSizeRow = CreatePairedInputRow("Size:");
        barOffsetXInput = CreateCompactNumericInput("X:", config.BarOffsetX, -500, 500, value => config.BarOffsetX = value);
        barOffsetYInput = CreateCompactNumericInput("Y:", config.BarOffsetY, -500, 500, value => config.BarOffsetY = value);
        barWidthInput = CreateCompactNumericInput("Width:", config.BarWidth, 1, 500, value => config.BarWidth = value);
        barHeightInput = CreateCompactNumericInput("Height:", config.BarHeight, 1, 100, value => config.BarHeight = value);
        barOffsetRow.AddInputPair(barOffsetXInput, barOffsetYInput);
        barSizeRow.AddInputPair(barWidthInput, barHeightInput);

        barTypeDropdown = new LabeledEnumDropdownNode<ProgressBarType>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Bar Type:",
            Options = ProgressBarTypeOptions.Ordered(),
            SelectedOption = config.BarType,
            OnOptionSelected = value =>
            {
                config.BarType = value;
                SaveAndUpdate();
            },
        };

        barFillRightToLeftToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Fill Right To Left",
            IsChecked = config.BarFillRightToLeft,
            OnClick = isChecked =>
            {
                config.BarFillRightToLeft = isChecked;
                SaveAndUpdate();
            },
        };

        barColorModeDropdown = new LabeledEnumDropdownNode<ColorMode>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Bar Color Mode:",
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
            Size = new Vector2(360, ControlHeight),
            LabelText = "Bar Tint:",
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
            Label = "Bar Color: ",
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
            Label = "Bar Background: ",
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

        showRaidDpsToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = "Show Raid DPS",
            IsChecked = config.ShowRaidDps,
            TextTooltip = "Shows encounter-wide DPS next to the party-list header.",
            OnClick = isChecked =>
            {
                config.ShowRaidDps = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        raidFormatRow = new HorizontalListNode
        {
            Size = new Vector2(420, ControlHeight),
            ItemSpacing = 2.0f,
        };

        raidFormatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Raid Format:",
            Text = config.RaidDpsFormat,
            Placeholder = "[encdps:c.1]",
            OnInputComplete = value =>
            {
                config.RaidDpsFormat = value.ToString();
                SaveAndUpdate();
            },
        };

        raidFormatHelpButton = CreateFormatHelpButton();
        raidBrowseTagButton = CreateTagBrowserButton(() => OpenTagPicker(raidFormatInput));
        raidFormatRow.AddNode(new NodeBase[] { raidFormatInput, raidFormatHelpButton, raidBrowseTagButton });

        raidOffsetRow = CreatePairedInputRow("Position:");
        raidSizeRow = CreatePairedInputRow("Size:");
        raidOffsetXInput = CreateCompactNumericInput("X:", config.RaidOffsetX, -500, 500, value => config.RaidOffsetX = value);
        raidOffsetYInput = CreateCompactNumericInput("Y:", config.RaidOffsetY, -500, 500, value => config.RaidOffsetY = value);
        raidWidthInput = CreateCompactNumericInput("Width:", config.RaidWidth, 1, 500, value => config.RaidWidth = value);
        raidHeightInput = CreateCompactNumericInput("Height:", config.RaidHeight, 1, 100, value => config.RaidHeight = value);
        raidOffsetRow.AddInputPair(raidOffsetXInput, raidOffsetYInput);
        raidSizeRow.AddInputPair(raidWidthInput, raidHeightInput);

        raidAlignmentDropdown = new LabeledEnumDropdownNode<AlignmentType>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Raid Alignment:",
            Options = Enum.GetValues<AlignmentType>().ToList(),
            SelectedOption = config.RaidAlignment,
            OnOptionSelected = value =>
            {
                config.RaidAlignment = value;
                SaveAndUpdate();
            },
        };

        raidFontSizeInput = CreateNumericInput("Raid Font Size:", (int)config.RaidFontSize, 6, 72, value => config.RaidFontSize = (uint)value);

        raidFontTypeDropdown = new LabeledEnumDropdownNode<FontType>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Raid Font:",
            Options = Enum.GetValues<FontType>().ToList(),
            SelectedOption = config.RaidFontType,
            OnOptionSelected = value =>
            {
                config.RaidFontType = value;
                SaveAndUpdate();
            },
        };

        raidTextFlagsInput = new LabeledTextFlagsInputNode
        {
            Width = 360,
            LabelText = "Raid Text Style:",
            OnValueChanged = value =>
            {
                config.RaidTextFlags = value;
                SaveAndUpdate();
            },
        };

        raidTextColorInput = new ColorInputRow
        {
            Label = "Raid Text Color: ",
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().RaidTextColor,
            CurrentColor = config.RaidTextColor,
            OnColorConfirmed = color =>
            {
                config.RaidTextColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.RaidTextColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.RaidTextColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        raidOutlineColorInput = new ColorInputRow
        {
            Label = "Raid Outline Color: ",
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().RaidTextOutlineColor,
            CurrentColor = config.RaidTextOutlineColor,
            OnColorConfirmed = color =>
            {
                config.RaidTextOutlineColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.RaidTextOutlineColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.RaidTextOutlineColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        fontTypeDropdown = new LabeledEnumDropdownNode<FontType>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Font:",
            Options = Enum.GetValues<FontType>().ToList(),
            SelectedOption = config.FontType,
            OnOptionSelected = value =>
            {
                config.FontType = value;
                SaveAndUpdate();
            },
        };

        textFlagsInput = new LabeledTextFlagsInputNode
        {
            Width = 360,
            LabelText = "Text Style:",
            OnValueChanged = value =>
            {
                config.TextFlags = value;
                SaveAndUpdate();
            },
        };

        textColorInput = new ColorInputRow
        {
            Label = "Text Color: ",
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().TextColor,
            CurrentColor = config.TextColor,
            OnColorConfirmed = color =>
            {
                config.TextColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.TextColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.TextColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        outlineColorInput = new ColorInputRow
        {
            Label = "Outline Color: ",
            Size = new Vector2(Width, ControlHeight),
            DefaultColor = new PartyListMeterSettings().TextOutlineColor,
            CurrentColor = config.TextOutlineColor,
            OnColorConfirmed = color =>
            {
                config.TextOutlineColor = color;
                SaveAndUpdate();
            },
            OnColorCanceled = color =>
            {
                config.TextOutlineColor = color;
                SaveAndUpdate();
            },
            OnColorPreviewed = color =>
            {
                config.TextOutlineColor = color;
                System.PartyListMeterManager?.UpdateSettings();
            },
        };

        var resetRow = new HorizontalListNode
        {
            Size = new Vector2(360.0f, ResetButtonHeight),
        };
        resetRow.AddNode(resetButton);

        AddNode(1, new NodeBase[]
        {
            enabledToggle,
            resetRow,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Member Text",
        });
        AddNode(1, new NodeBase[]
        {
            showSelfToggle,
            showPartyToggle,
            hideWhenNoDataToggle,
            formatRow,
            anchorDropdown,
            memberOffsetRow,
            memberSizeRow,
            memberAlignmentDropdown,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Top Member",
        });
        AddNode(1, new NodeBase[]
        {
            highlightTopMemberToggle,
            topMemberStatDropdown,
            topMemberFormatRow,
            topMemberTextColorInput,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Member Bar",
        });
        AddNode(1, new NodeBase[]
        {
            showMemberBarsToggle,
            barOffsetRow,
            barSizeRow,
            barTypeDropdown,
            barFillRightToLeftToggle,
            barColorModeDropdown,
            barColorTreatmentDropdown,
            barColorInput,
            barBackgroundColorInput,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Raid DPS",
        });
        AddNode(1, new NodeBase[]
        {
            showRaidDpsToggle,
            raidFormatRow,
            raidOffsetRow,
            raidSizeRow,
            raidAlignmentDropdown,
            raidFontSizeInput,
            raidFontTypeDropdown,
            raidTextFlagsInput,
            raidTextColorInput,
            raidOutlineColorInput,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Member Text Style",
        });
        AddNode(1, new NodeBase[]
        {
            fontSizeInput,
            fontTypeDropdown,
            textFlagsInput,
            textColorInput,
            outlineColorInput,
        });

        RefreshControls();
    }
}
