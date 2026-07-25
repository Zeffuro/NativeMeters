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
    private HorizontalListNode memberOffsetRow = null!;
    private HorizontalListNode memberSizeRow = null!;
    private LabeledNumericInputNode offsetXInput = null!;
    private LabeledNumericInputNode offsetYInput = null!;
    private LabeledNumericInputNode widthInput = null!;
    private LabeledNumericInputNode heightInput = null!;
    private LabeledNumericInputNode fontSizeInput = null!;
    private LabeledEnumDropdownNode<FontType> fontTypeDropdown = null!;
    private LabeledEnumDropdownNode<TextFlags> textFlagsDropdown = null!;
    private LabeledEnumDropdownNode<AlignmentType> memberAlignmentDropdown = null!;
    private ColorInputRow textColorInput = null!;
    private ColorInputRow outlineColorInput = null!;

    private CheckboxRowNode showSelfToggle = null!;
    private CheckboxRowNode showPartyToggle = null!;
    private CheckboxRowNode hideWhenNoDataToggle = null!;
    private LabeledEnumDropdownNode<PartyListMeterAnchor> anchorDropdown = null!;
    private CheckboxRowNode showMemberBarsToggle = null!;
    private HorizontalListNode barOffsetRow = null!;
    private HorizontalListNode barSizeRow = null!;
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
    private HorizontalListNode raidOffsetRow = null!;
    private HorizontalListNode raidSizeRow = null!;
    private LabeledNumericInputNode raidOffsetXInput = null!;
    private LabeledNumericInputNode raidOffsetYInput = null!;
    private LabeledNumericInputNode raidWidthInput = null!;
    private LabeledNumericInputNode raidHeightInput = null!;
    private LabeledEnumDropdownNode<AlignmentType> raidAlignmentDropdown = null!;

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

        memberOffsetRow = CreatePairedInputRow();
        memberSizeRow = CreatePairedInputRow();
        offsetXInput = CreateCompactNumericInput("X:", config.OffsetX, -500, 500, value => config.OffsetX = value);
        offsetYInput = CreateCompactNumericInput("Y:", config.OffsetY, -500, 500, value => config.OffsetY = value);
        widthInput = CreateCompactNumericInput("Width:", config.Width, 1, 500, value => config.Width = value);
        heightInput = CreateCompactNumericInput("Height:", config.Height, 1, 100, value => config.Height = value);
        fontSizeInput = CreateNumericInput("Font Size:", (int)config.FontSize, 6, 72, value => config.FontSize = (uint)value);
        memberOffsetRow.AddNode(new NodeBase[] { offsetXInput, offsetYInput });
        memberSizeRow.AddNode(new NodeBase[] { widthInput, heightInput });

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

        barOffsetRow = CreatePairedInputRow();
        barSizeRow = CreatePairedInputRow();
        barOffsetXInput = CreateCompactNumericInput("X:", config.BarOffsetX, -500, 500, value => config.BarOffsetX = value);
        barOffsetYInput = CreateCompactNumericInput("Y:", config.BarOffsetY, -500, 500, value => config.BarOffsetY = value);
        barWidthInput = CreateCompactNumericInput("Width:", config.BarWidth, 1, 500, value => config.BarWidth = value);
        barHeightInput = CreateCompactNumericInput("Height:", config.BarHeight, 1, 100, value => config.BarHeight = value);
        barOffsetRow.AddNode(new NodeBase[] { barOffsetXInput, barOffsetYInput });
        barSizeRow.AddNode(new NodeBase[] { barWidthInput, barHeightInput });

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

        raidOffsetRow = CreatePairedInputRow();
        raidSizeRow = CreatePairedInputRow();
        raidOffsetXInput = CreateCompactNumericInput("X:", config.RaidOffsetX, -500, 500, value => config.RaidOffsetX = value);
        raidOffsetYInput = CreateCompactNumericInput("Y:", config.RaidOffsetY, -500, 500, value => config.RaidOffsetY = value);
        raidWidthInput = CreateCompactNumericInput("Width:", config.RaidWidth, 1, 500, value => config.RaidWidth = value);
        raidHeightInput = CreateCompactNumericInput("Height:", config.RaidHeight, 1, 100, value => config.RaidHeight = value);
        raidOffsetRow.AddNode(new NodeBase[] { raidOffsetXInput, raidOffsetYInput });
        raidSizeRow.AddNode(new NodeBase[] { raidWidthInput, raidHeightInput });

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

        textFlagsDropdown = new LabeledEnumDropdownNode<TextFlags>
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = "Text Style:",
            Options = Enum.GetValues<TextFlags>().ToList(),
            SelectedOption = config.TextFlags,
            OnOptionSelected = value =>
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
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = "Shared Text Style",
        });
        AddNode(1, new NodeBase[]
        {
            fontSizeInput,
            fontTypeDropdown,
            textFlagsDropdown,
            textColorInput,
            outlineColorInput,
        });

        RefreshControls();
    }
}
