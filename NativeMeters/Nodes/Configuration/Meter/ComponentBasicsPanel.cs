using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentBasicsPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabeledTextInputNode nameInput;
    private readonly LabeledTextInputNode sourceInput;
    private readonly LabeledDropdownNode tagEnumDropdown;
    private readonly LabeledDropdownNode barStatDropdown;
    private readonly LabeledEnumDropdownNode<JobIconType> jobIconTypeEnumDropdown;
    private readonly LabeledNumericInputNode posXInput;
    private readonly LabeledNumericInputNode posYInput;
    private readonly LabeledNumericInputNode widthInput;
    private readonly LabeledNumericInputNode heightInput;
    private readonly LabeledNumericInputNode zIndexInput;

    public Action? OnSettingsChanged { get; set; }
    public Action<string>? OnNameChanged { get; set; }
    public Action? OnLayoutChanged { get; set; }

    private bool isLoading;

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
                                 "Example: [name_first.1].: [dps:k.1] -> J.: 12.3k";

        nameInput = new LabeledTextInputNode
        {
            LabelText = "Display Name: ",
            Size = new Vector2(Width, 28),
            OnInputComplete = val =>
            {
                if (settings == null) return;
                settings.Name = val.ToString();
                OnNameChanged?.Invoke(val.ToString());
                OnSettingsChanged?.Invoke();
            }
        };

        sourceInput = new LabeledTextInputNode
        {
            LabelText = "Format String: ",
            Size = new Vector2(Width, 28),
            Placeholder = "[name] [dps]",
            TextTooltip = tagTooltip,
            OnInputComplete = val =>
            {
                if (settings == null) return;
                settings.DataSource = val.ToString();
                OnSettingsChanged?.Invoke();
            }
        };

        tagEnumDropdown = new LabeledDropdownNode
        {
            LabelText = "Insert Tag: ",
            Size = new Vector2(Width, 28),
            Options = TagDefinitions.GetLabels(),
            MaxListOptions = 10,
            SelectedOption = TagDefinitions.DefaultDropdownLabel,
            TextTooltip = tagTooltip,
            OnOptionSelected = selected =>
            {
                if (TagDefinitions.Templates.TryGetValue(selected, out var tag) && !string.IsNullOrEmpty(tag))
                {
                    sourceInput.Text += tag;
                    sourceInput.InnerInput.OnInputComplete?.Invoke(sourceInput.Text);
                    tagEnumDropdown?.SelectedOption = TagDefinitions.DefaultDropdownLabel;
                }
            }
        };

        barStatDropdown = new LabeledDropdownNode
        {
            LabelText = "Fill Stat: ",
            Size = new Vector2(Width, 28),
            Options = CombatantStatHelpers.GetAvailableStatSelectors(),
            OnOptionSelected = val =>
            {
                if (settings == null) return;
                settings.DataSource = val;
                OnSettingsChanged?.Invoke();
            }
        };

        jobIconTypeEnumDropdown = new LabeledEnumDropdownNode<JobIconType>
        {
            LabelText = "Icon Style: ",
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<JobIconType>().ToList(),
            OnOptionSelected = val =>
            {
                settings?.JobIconType = val;
                OnSettingsChanged?.Invoke();
            }
        };

        var posRow = new HorizontalListNode { Size = new Vector2(Width, 30), ItemSpacing = 8.0f };
        posXInput = new LabeledNumericInputNode
        {
            LabelText = "Pos X:",
            Size = new Vector2(166, 28),
            Step = 10,
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Position = settings.Position with { X = val };
                OnSettingsChanged?.Invoke();
            }
        };
        posYInput = new LabeledNumericInputNode
        {
            LabelText = "Pos Y:",
            Size = new Vector2(166, 28),
            Step = 10,
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Position = settings.Position with { Y = val };
                OnSettingsChanged?.Invoke();
            }
        };
        posRow.AddNode([posXInput, posYInput]);

        var sizeRow = new HorizontalListNode { Size = new Vector2(Width, 30), ItemSpacing = 8.0f };
        widthInput = new LabeledNumericInputNode
        {
            LabelText = "Width:",
            Size = new Vector2(166, 28),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Size = settings.Size with { X = val };
                OnSettingsChanged?.Invoke();
            }
        };
        heightInput = new LabeledNumericInputNode
        {
            LabelText = "Height:",
            Size = new Vector2(166, 28),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.Size = settings.Size with { Y = val };
                OnSettingsChanged?.Invoke();
            }
        };
        sizeRow.AddNode([widthInput, heightInput]);

        zIndexInput = new LabeledNumericInputNode
        {
            LabelText = "Z-Order:",
            Size = new Vector2(Width, 28),
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.ZIndex = val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([nameInput, sourceInput, tagEnumDropdown, barStatDropdown, jobIconTypeEnumDropdown, posRow, sizeRow, zIndexInput]);
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        nameInput.Text = settings.Name;
        sourceInput.Text = settings.DataSource;
        posXInput.Value = (int)settings.Position.X;
        posYInput.Value = (int)settings.Position.Y;
        widthInput.Value = (int)settings.Size.X;
        heightInput.Value = (int)settings.Size.Y;
        zIndexInput.Value = settings.ZIndex;

        var isText = settings.Type == MeterComponentType.Text;
        var isBar = settings.Type == MeterComponentType.ProgressBar;
        var isIcon = settings.Type == MeterComponentType.JobIcon;

        sourceInput.IsVisible = isText;
        tagEnumDropdown.IsVisible = isText;
        barStatDropdown.IsVisible = isBar;
        jobIconTypeEnumDropdown.IsVisible = isIcon;

        if (isBar) barStatDropdown.SelectedOption = settings.DataSource;
        if (isIcon) jobIconTypeEnumDropdown.SelectedOption = settings.JobIconType;

        isLoading = false;
        RecalculateLayout();
        OnLayoutChanged?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        nameInput.Width = Width;
        sourceInput.Width = Width;
        tagEnumDropdown.Width = Width;
        barStatDropdown.Width = Width;
        jobIconTypeEnumDropdown.Width = Width;
        zIndexInput.Width = Width;
    }
}
