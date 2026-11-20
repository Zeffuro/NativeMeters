using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
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
    private const float CheckboxHeight = 20.0f;
    private const float ControlHeight = 24.0f;
    private const float FormatRowHeight = 28.0f;
    private const float FormatButtonSize = 20.0f;
    private const float ResetButtonHeight = 24.0f;

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
            String = Loc.Get("PartyListMeter"),
        });

        BuildGeneralControls();
        BuildMemberTextControls();
        BuildTopMemberControls();
        BuildMemberBarControls();
        BuildRaidDpsControls();
        BuildMemberTextStyleControls();
        AddSections();
        RefreshControls();
    }

    private void BuildGeneralControls()
    {
        enabledToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("GeneralEnabled"),
            IsChecked = config.Enabled,
            TextTooltip = Loc.Get("PartyListEnableHelp"),
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
            String = Loc.Get("GeneralReset"),
            TextNode = { TextColor = ColorHelper.GetColor(50) },
            UnlockAfterClick = true,
            TextTooltip = Loc.Get("PartyListResetHelp"),
            OnClick = ResetPartyListSettings,
        };
    }

    private void BuildMemberTextControls()
    {
        showSelfToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("PartyListShowSelf"),
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
            String = Loc.Get("PartyListShowMembers"),
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
            String = Loc.Get("PartyListHideEmpty"),
            IsChecked = config.HideWhenNoCombatData,
            OnClick = isChecked =>
            {
                config.HideWhenNoCombatData = isChecked;
                SaveAndUpdate();
            },
        };

        formatRow = new HorizontalListNode
        {
            Size = new Vector2(420, FormatRowHeight),
            ItemSpacing = 2.0f,
        };

        formatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, FormatRowHeight),
            LabelText = Loc.Get("PartyListFormat"),
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
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListAnchor"),
            Options = Enum.GetValues<PartyListMeterAnchor>().ToList(),
            SelectedOption = config.Anchor,
            OnOptionSelected = value =>
            {
                config.Anchor = value;
                SaveAndUpdate();
            },
        };

        memberOffsetRow = CreatePairedInputRow(Loc.Get("PartyListPosition"));
        memberSizeRow = CreatePairedInputRow(Loc.Get("PartyListSize"));
        offsetXInput = CreateCompactNumericInput(Loc.Get("PartyListPosX"), config.OffsetX, -500, 500, value => config.OffsetX = value);
        offsetYInput = CreateCompactNumericInput(Loc.Get("PartyListPosY"), config.OffsetY, -500, 500, value => config.OffsetY = value);
        widthInput = CreateCompactNumericInput(Loc.Get("PartyListWidth"), config.Width, 1, 500, value => config.Width = value);
        heightInput = CreateCompactNumericInput(Loc.Get("PartyListHeight"), config.Height, 1, 100, value => config.Height = value);
        memberOffsetRow.AddInputPair(offsetXInput, offsetYInput);
        memberSizeRow.AddInputPair(widthInput, heightInput);

        memberAlignmentDropdown = new LabeledEnumDropdownNode<AlignmentType>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListAlignment"),
            Options = Enum.GetValues<AlignmentType>().ToList(),
            SelectedOption = config.MemberAlignment,
            OnOptionSelected = value =>
            {
                config.MemberAlignment = value;
                SaveAndUpdate();
            },
        };
    }

    private void BuildTopMemberControls()
    {
        highlightTopMemberToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("PartyListHighlightTop"),
            IsChecked = config.HighlightTopMember,
            TextTooltip = Loc.Get("PartyListHighlightTopHelp"),
            OnClick = isChecked =>
            {
                config.HighlightTopMember = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        topMemberStatDropdown = new LabeledDropdownNode
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListTopStat"),
            GetOptionLabel = Loc.StatLabel,
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
            Size = new Vector2(420, FormatRowHeight),
            ItemSpacing = 2.0f,
        };

        topMemberFormatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, FormatRowHeight),
            LabelText = Loc.Get("PartyListTopFormat"),
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
            Label = Loc.Get("PartyListTopColor"),
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
    }

    private void BuildRaidDpsControls()
    {
        showRaidDpsToggle = new CheckboxRowNode
        {
            Size = new Vector2(360.0f, CheckboxHeight),
            IsVisible = true,
            String = Loc.Get("PartyListShowRaid"),
            IsChecked = config.ShowRaidDps,
            TextTooltip = Loc.Get("PartyListRaidHelp"),
            OnClick = isChecked =>
            {
                config.ShowRaidDps = isChecked;
                SaveAndUpdate();
                ApplyEnabledState();
            },
        };

        raidFormatRow = new HorizontalListNode
        {
            Size = new Vector2(420, FormatRowHeight),
            ItemSpacing = 2.0f,
        };

        raidFormatInput = new LabeledTextInputNode
        {
            Size = new Vector2(360, FormatRowHeight),
            LabelText = Loc.Get("PartyListRaidFormat"),
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

        raidOffsetRow = CreatePairedInputRow(Loc.Get("PartyListPosition"));
        raidSizeRow = CreatePairedInputRow(Loc.Get("PartyListSize"));
        raidOffsetXInput = CreateCompactNumericInput(Loc.Get("PartyListPosX"), config.RaidOffsetX, -500, 500, value => config.RaidOffsetX = value);
        raidOffsetYInput = CreateCompactNumericInput(Loc.Get("PartyListPosY"), config.RaidOffsetY, -500, 500, value => config.RaidOffsetY = value);
        raidWidthInput = CreateCompactNumericInput(Loc.Get("PartyListWidth"), config.RaidWidth, 1, 500, value => config.RaidWidth = value);
        raidHeightInput = CreateCompactNumericInput(Loc.Get("PartyListHeight"), config.RaidHeight, 1, 100, value => config.RaidHeight = value);
        raidOffsetRow.AddInputPair(raidOffsetXInput, raidOffsetYInput);
        raidSizeRow.AddInputPair(raidWidthInput, raidHeightInput);

        raidAlignmentDropdown = new LabeledEnumDropdownNode<AlignmentType>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListRaidAlignment"),
            Options = Enum.GetValues<AlignmentType>().ToList(),
            SelectedOption = config.RaidAlignment,
            OnOptionSelected = value =>
            {
                config.RaidAlignment = value;
                SaveAndUpdate();
            },
        };

        raidFontSizeInput = CreateNumericInput(Loc.Get("PartyListRaidFontSize"), (int)config.RaidFontSize, 6, 72, value => config.RaidFontSize = (uint)value);

        raidFontTypeDropdown = new LabeledEnumDropdownNode<FontType>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListRaidFont"),
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
            LabelText = Loc.Get("PartyListRaidStyle"),
            OnValueChanged = value =>
            {
                config.RaidTextFlags = value;
                SaveAndUpdate();
            },
        };

        raidTextColorInput = new ColorInputRow
        {
            Label = Loc.Get("PartyListRaidColor"),
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
            Label = Loc.Get("PartyListRaidOutline"),
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
    }

    private void BuildMemberTextStyleControls()
    {
        fontSizeInput = CreateNumericInput(Loc.Get("PartyListFontSize"), (int)config.FontSize, 6, 72, value => config.FontSize = (uint)value);

        fontTypeDropdown = new LabeledEnumDropdownNode<FontType>
        {
            ShowStepButtons = true,
            Size = new Vector2(360, ControlHeight),
            LabelText = Loc.Get("PartyListFont"),
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
            LabelText = Loc.Get("PartyListTextStyle"),
            OnValueChanged = value =>
            {
                config.TextFlags = value;
                SaveAndUpdate();
            },
        };

        textColorInput = new ColorInputRow
        {
            Label = Loc.Get("PartyListTextColor"),
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
            Label = Loc.Get("PartyListOutline"),
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
    }

    private void AddSections()
    {
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

        AddNode(new ResNode { Height = 10 });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = Loc.Get("PartyListMemberText"),
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
            String = Loc.Get("PartyListTopMember"),
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
            String = Loc.Get("PartyListMemberBar"),
        });
        AddNode(1, new NodeBase[]
        {
            showMemberBarsToggle,
            barTypeDropdown,
            barSizingDropdown,
            new ResNode { Height = 4 },
            barFillStatDropdown,
            barFillRightToLeftToggle,
            new ResNode { Height = 12 },
            barColorModeDropdown,
            barColorTreatmentDropdown,
            barColorInput,
            barBackgroundColorInput,
            new ResNode { Height = 4 },
            barSizeRow,
            barOffsetRow,
        });

        AddNode(new CategoryTextNode
        {
            Height = CategoryHeight,
            String = Loc.Get("PartyListRaidDps"),
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
            String = Loc.Get("PartyListMemberStyle"),
        });
        AddNode(1, new NodeBase[]
        {
            fontSizeInput,
            fontTypeDropdown,
            textFlagsInput,
            textColorInput,
            outlineColorInput,
        });
    }
}
