using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Models;
using NativeMeters.Nodes.Configuration.Meter.Search;
using NativeMeters.Services;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

public sealed class ComponentBasicsPanel : VerticalListNode
{
    private const float InputHeight = 28.0f;
    private const float RowHeight = 30.0f;
    private const float ButtonSize = 24.0f;

    private ComponentSettings? settings;

    private readonly ComponentTextInputRowNode nameInput;
    private readonly HorizontalListNode sourceRow;
    private readonly ComponentTextInputRowNode sourceInput;
    private readonly CircleButtonNode formatHelpButton;
    private readonly CircleButtonNode browseTagButton;
    private readonly ComponentStringDropdownRowNode barStatDropdown;
    private readonly ComponentEnumDropdownRowNode<JobIconType> jobIconTypeEnumDropdown;

    private readonly HorizontalListNode iconRow;
    private readonly ComponentNumericInputRowNode iconIdInput;
    private readonly CircleButtonNode browseIconButton;

    private readonly HorizontalListNode posRow;
    private readonly ComponentNumericInputRowNode posXInput;
    private readonly ComponentNumericInputRowNode posYInput;
    private readonly HorizontalListNode sizeRow;
    private readonly ComponentNumericInputRowNode widthInput;
    private readonly ComponentNumericInputRowNode heightInput;
    private readonly ComponentNumericInputRowNode zIndexInput;

    public Action? OnSettingsChanged { get; set; }
    public Action<string>? OnNameChanged { get; set; }
    public Action? OnLayoutChanged { get; set; }

    private bool isLoading;
    private bool isDisposed;

    private Action<string>? currentTagInsertAction;
    private Action<TagInfo>? currentTagSelectionAction;
    private Action<uint>? currentIconSelectionAction;

    public ComponentBasicsPanel()
    {
        FitContents = true;
        ItemSpacing = 4.0f;

        const string tagTooltip = "Format syntax: [tag_part: modifier.precision]\n" +
                                 "• : r = Raw (no commas)\n" +
                                 "• :k/: m = Kilo/Mega units\n" +
                                 "• .N = Decimals or Text length\n" +
                                 "• _first/_last = Name parts\n" +
                                 "• _skill/_val = MaxHit parts\n\n" +
                                 "Example: [name_first.1].:[dps:k.1] -> J.: 12.3k";

        nameInput = new ComponentTextInputRowNode
        {
            LabelText = "Display Name: ",
            Size = new Vector2(Width, InputHeight),
            OnInputComplete = val =>
            {
                if (settings == null) return;
                settings.Name = val.ToString();
                OnNameChanged?.Invoke(val.ToString());
                OnSettingsChanged?.Invoke();
            }
        };

        sourceRow = new HorizontalListNode { Size = new Vector2(Width, RowHeight), ItemSpacing = 2.0f };

        sourceInput = new ComponentTextInputRowNode
        {
            LabelText = "Format String: ",
            Size = new Vector2(0, InputHeight),
            Placeholder = "[name] [dps]",
            OnInputComplete = val =>
            {
                if (settings == null) return;
                settings.DataSource = val.ToString();
                OnSettingsChanged?.Invoke();
            }
        };

        formatHelpButton = new CircleButtonNode
        {
            Icon = CircleButtonIcon.QuestionMark,
            Size = new Vector2(ButtonSize),
            Y = (RowHeight - ButtonSize) / 2.0f,
            TextTooltip = tagTooltip,
        };

        browseTagButton = new CircleButtonNode()
        {
            Icon = CircleButtonIcon.MagnifyingGlass,
            Size = new Vector2(ButtonSize),
            Y = (RowHeight - ButtonSize) / 2.0f,
            OnClick = OpenTagPicker
        };

        sourceRow.AddNode([sourceInput, formatHelpButton, browseTagButton]);

        barStatDropdown = new ComponentStringDropdownRowNode
        {
            LabelText = "Fill Stat: ",
            Size = new Vector2(Width, InputHeight),
            Options = StatSelector.GetAvailableStatSelectors(),
            OnOptionSelected = val =>
            {
                if (settings == null) return;
                settings.DataSource = val;
                OnSettingsChanged?.Invoke();
            }
        };

        jobIconTypeEnumDropdown = new ComponentEnumDropdownRowNode<JobIconType>
        {
            LabelText = "Icon Style: ",
            Size = new Vector2(Width, InputHeight),
            Options = Enum.GetValues<JobIconType>().ToList(),
            OnOptionSelected = val =>
            {
                settings?.JobIconType = val;
                OnSettingsChanged?.Invoke();
            }
        };

        iconIdInput = new ComponentNumericInputRowNode
        {
            LabelText = "Icon ID:",
            Size = new Vector2(200, InputHeight),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.IconId = (uint)val;
                OnSettingsChanged?.Invoke();
            }
        };

        browseIconButton = new CircleButtonNode()
        {
            Icon = CircleButtonIcon.MagnifyingGlass,
            Size = new Vector2(InputHeight, InputHeight),
            OnClick = OpenIconPicker
        };

        iconRow = new HorizontalListNode { Size = new Vector2(Width, RowHeight), ItemSpacing = 8.0f };
        iconRow.AddNode([iconIdInput, browseIconButton]);

        posRow = new HorizontalListNode { Size = new Vector2(Width, RowHeight), ItemSpacing = 8.0f };
        posXInput = new ComponentNumericInputRowNode
        {
            LabelText = "Pos X:",
            Size = new Vector2(166, InputHeight),
            Step = 10,
            Min = -100,
            Max = 1000,
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Position = settings.Position with { X = val };
                OnSettingsChanged?.Invoke();
            }
        };
        posYInput = new ComponentNumericInputRowNode
        {
            LabelText = "Pos Y:",
            Size = new Vector2(166, InputHeight),
            Step = 10,
            Min = -100,
            Max = 1000,
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Position = settings.Position with { Y = val };
                OnSettingsChanged?.Invoke();
            }
        };
        posRow.AddNode([posXInput, posYInput]);

        sizeRow = new HorizontalListNode { Size = new Vector2(Width, RowHeight), ItemSpacing = 8.0f };
        widthInput = new ComponentNumericInputRowNode
        {
            LabelText = "Width:",
            Size = new Vector2(166, InputHeight),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Size = settings.Size with { X = val };
                OnSettingsChanged?.Invoke();
            },
        };
        heightInput = new ComponentNumericInputRowNode
        {
            LabelText = "Height:",
            Size = new Vector2(166, InputHeight),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Size = settings.Size with { Y = val };
                OnSettingsChanged?.Invoke();
            },
        };
        sizeRow.AddNode([widthInput, heightInput]);

        zIndexInput = new ComponentNumericInputRowNode
        {
            LabelText = "Z-Order:",
            Size = new Vector2(Width, InputHeight),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.ZIndex = val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([nameInput, sourceRow, barStatDropdown, jobIconTypeEnumDropdown, iconRow, posRow, sizeRow, zIndexInput]);
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;

            if (value)
            {
                ApplyVisibility();
            }
            else
            {
                SetAllChildVisibility(false);
            }
        }
    }

    private void OpenIconPicker()
    {
        IconRegistry.Initialize(Service.DataManager);

        currentIconSelectionAction = iconId =>
        {
            if (isDisposed || settings == null) return;
            settings.IconId = iconId;
            iconIdInput.Value = (int)iconId;
            OnSettingsChanged?.Invoke();
        };

        System.IconSearchAddon.SelectionResult = currentIconSelectionAction;
        System.IconSearchAddon.Open();
    }

    private void OpenTagPicker()
    {
        currentTagInsertAction = tagString =>
        {
            if (isDisposed || settings == null) return;
            sourceInput.Text += tagString;
            sourceInput.InnerInput.OnInputComplete?.Invoke(sourceInput.Text);
        };

        currentTagSelectionAction = tagInfo =>
        {
            if (isDisposed || settings == null) return;
            sourceInput.Text += tagInfo.Tag;
            sourceInput.InnerInput.OnInputComplete?.Invoke(sourceInput.Text);
        };

        System.TagSearchAddon.OnInsertClicked = currentTagInsertAction;
        System.TagSearchAddon.SelectionResult = currentTagSelectionAction;
        System.TagSearchAddon.Open();
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        nameInput.Text = settings.Name;
        sourceInput.Text = settings.DataSource;
        iconIdInput.Value = (int)settings.IconId;
        posXInput.Value = (int)settings.Position.X;
        posYInput.Value = (int)settings.Position.Y;
        widthInput.Value = (int)settings.Size.X;
        heightInput.Value = (int)settings.Size.Y;
        zIndexInput.Value = settings.ZIndex;

        ApplyVisibility();

        if (settings.Type == MeterComponentType.ProgressBar) barStatDropdown.SelectedOption = settings.DataSource;
        if (settings.Type == MeterComponentType.JobIcon) jobIconTypeEnumDropdown.SelectedOption = settings.JobIconType;

        isLoading = false;
        RecalculateLayout();
        OnLayoutChanged?.Invoke();
    }

    private void ApplyVisibility()
    {
        var isText = settings?.Type == MeterComponentType.Text;
        var isBar = settings?.Type == MeterComponentType.ProgressBar;
        var isJobIcon = settings?.Type == MeterComponentType.JobIcon;
        var isIcon = settings?.Type == MeterComponentType.Icon;

        nameInput.IsVisible = true;
        SetSourceRowVisible(isText);
        barStatDropdown.IsVisible = isBar;
        jobIconTypeEnumDropdown.IsVisible = isJobIcon;
        SetIconRowVisible(isIcon);
        posRow.IsVisible = true;
        posXInput.IsVisible = true;
        posYInput.IsVisible = true;
        sizeRow.IsVisible = true;
        widthInput.IsVisible = true;
        heightInput.IsVisible = true;
        zIndexInput.IsVisible = true;
    }

    private void SetAllChildVisibility(bool isVisible)
    {
        nameInput.IsVisible = isVisible;
        SetSourceRowVisible(isVisible);
        barStatDropdown.IsVisible = isVisible;
        jobIconTypeEnumDropdown.IsVisible = isVisible;
        SetIconRowVisible(isVisible);
        posRow.IsVisible = isVisible;
        posXInput.IsVisible = isVisible;
        posYInput.IsVisible = isVisible;
        sizeRow.IsVisible = isVisible;
        widthInput.IsVisible = isVisible;
        heightInput.IsVisible = isVisible;
        zIndexInput.IsVisible = isVisible;
    }

    private void SetSourceRowVisible(bool isVisible)
    {
        sourceRow.IsVisible = isVisible;
        sourceInput.IsVisible = isVisible;
        formatHelpButton.IsVisible = isVisible;
        formatHelpButton.ImageNode.IsVisible = isVisible;
        browseTagButton.IsVisible = isVisible;
        browseTagButton.ImageNode.IsVisible = isVisible;
    }

    private void SetIconRowVisible(bool isVisible)
    {
        iconRow.IsVisible = isVisible;
        iconIdInput.IsVisible = isVisible;
        browseIconButton.IsVisible = isVisible;
        browseIconButton.ImageNode.IsVisible = isVisible;
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        UpdateChildWidths();
    }

    protected override void OnRecalculateLayout()
    {
        if (!IsVisible)
        {
            SetAllChildVisibility(false);
            Height = 0.0f;
            return;
        }

        ApplyVisibility();
        UpdateChildWidths();

        var yPosition = 0.0f;
        LayoutRow(nameInput, ref yPosition);
        LayoutRow(sourceRow, ref yPosition);
        LayoutRow(barStatDropdown, ref yPosition);
        LayoutRow(jobIconTypeEnumDropdown, ref yPosition);
        LayoutRow(iconRow, ref yPosition);
        LayoutRow(posRow, ref yPosition);
        LayoutRow(sizeRow, ref yPosition);
        LayoutRow(zIndexInput, ref yPosition);

        Height = Math.Max(0.0f, yPosition - ItemSpacing);
    }

    private void UpdateChildWidths()
    {
        nameInput.Size = new Vector2(Width, InputHeight);

        sourceRow.Size = new Vector2(Width, RowHeight);
        formatHelpButton.Size = new Vector2(ButtonSize);
        browseTagButton.Size = new Vector2(ButtonSize);
        var sourceButtonWidth = formatHelpButton.Width + browseTagButton.Width + sourceRow.ItemSpacing * 2.0f;
        sourceInput.Size = new Vector2(Math.Max(0.0f, Width - sourceButtonWidth), InputHeight);

        barStatDropdown.Size = new Vector2(Width, InputHeight);
        jobIconTypeEnumDropdown.Size = new Vector2(Width, InputHeight);

        iconRow.Size = new Vector2(Width, RowHeight);
        browseIconButton.Size = new Vector2(InputHeight, InputHeight);
        iconIdInput.Size = new Vector2(Math.Max(0.0f, Width - browseIconButton.Width - iconRow.ItemSpacing), InputHeight);

        var pairedInputWidth = Math.Max(0.0f, (Width - posRow.ItemSpacing) / 2.0f);
        posRow.Size = new Vector2(Width, RowHeight);
        posXInput.Size = new Vector2(pairedInputWidth, InputHeight);
        posYInput.Size = new Vector2(pairedInputWidth, InputHeight);

        sizeRow.Size = new Vector2(Width, RowHeight);
        widthInput.Size = new Vector2(pairedInputWidth, InputHeight);
        heightInput.Size = new Vector2(pairedInputWidth, InputHeight);

        zIndexInput.Size = new Vector2(Width, InputHeight);
    }

    private void LayoutRow(NodeBase node, ref float yPosition)
    {
        if (!node.IsVisible) return;

        node.Position = new Vector2(0.0f, yPosition);

        if (node is HorizontalListNode row)
        {
            CenterRowChildVerticalOffsets(row);
            row.RecalculateLayout();
            CenterRowChildVerticalOffsets(row);
        }

        yPosition += node.Height + ItemSpacing;
    }

    private static void CenterRowChildVerticalOffsets(HorizontalListNode row)
    {
        foreach (var child in row.Nodes)
        {
            child.Y = Math.Max(0.0f, (row.Height - child.Height) / 2.0f);
        }
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        isDisposed = true;
        var canCloseSearchAddons = disposing && !isNativeDestructor;

        if (System.TagSearchAddon != null)
        {
            if (System.TagSearchAddon.OnInsertClicked == currentTagInsertAction)
            {
                System.TagSearchAddon.OnInsertClicked = null;
                System.TagSearchAddon.SelectionResult = null;
                if (canCloseSearchAddons) System.TagSearchAddon.Close();
            }
        }

        if (System.IconSearchAddon != null)
        {
            if (System.IconSearchAddon.SelectionResult == currentIconSelectionAction)
            {
                System.IconSearchAddon.SelectionResult = null;
                if (canCloseSearchAddons) System.IconSearchAddon.Close();
            }
        }

        base.Dispose(disposing, isNativeDestructor);
    }
}
