using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

public sealed class ComponentTypographyPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly ComponentEnumDropdownRowNode<FontType> fontTypeEnumDropdown;
    private readonly LabeledTextFlagsInputNode textFlagsInput;
    private readonly ComponentEnumDropdownRowNode<AlignmentType> alignmentEnumDropdown;
    private readonly ComponentNumericInputRowNode fontSizeInput;

    public Action? OnSettingsChanged { get; set; }
    public Action? OnLayoutChanged { get; set; }

    private bool isLoading;

    public ComponentTypographyPanel()
    {
        FitContents = true;
        ItemSpacing = 4.0f;

        headerLabel = new LabelTextNode
        {
            String = "Typography",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        fontTypeEnumDropdown = new ComponentEnumDropdownRowNode<FontType>
        {
            LabelText = "Font:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<FontType>().ToList(),
            OnOptionSelected = val =>
            {
                settings?.FontType = val;
                OnSettingsChanged?.Invoke();
            }
        };

        textFlagsInput = new LabeledTextFlagsInputNode
        {
            LabelText = "Style:",
            LabelWidth = 128.0f,
            ControlSpacing = 0.0f,
            MaximumControlWidth = float.PositiveInfinity,
            OnValueChanged = val =>
            {
                settings?.TextFlags = val;
                OnSettingsChanged?.Invoke();
            }
        };

        alignmentEnumDropdown = new ComponentEnumDropdownRowNode<AlignmentType>
        {
            LabelText = "Alignment:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<AlignmentType>().ToList(),
            OnOptionSelected = val =>
            {
                settings?.AlignmentType = val;
                OnSettingsChanged?.Invoke();
            }
        };

        fontSizeInput = new ComponentNumericInputRowNode
        {
            LabelText = "Size:",
            Size = new Vector2(Width, 28),
            Min = 6,
            Max = 72,
            OnValueUpdate = val =>
            {
                if (settings == null || isLoading) return;
                settings.FontSize = (uint)val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([headerLabel, fontTypeEnumDropdown, alignmentEnumDropdown, fontSizeInput, textFlagsInput]);
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            SetAllChildVisibility(value);
        }
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        var isText = settings.Type == MeterComponentType.Text;
        var wasVisible = IsVisible;

        IsVisible = isText;

        if (isText)
        {
            fontTypeEnumDropdown.SelectedOption = settings.FontType;
            textFlagsInput.Value = settings.TextFlags;
            alignmentEnumDropdown.SelectedOption = settings.AlignmentType;
            fontSizeInput.Value = (int)settings.FontSize;

            RecalculateLayout();
        }

        isLoading = false;
        if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        headerLabel.Width = Width;
        fontTypeEnumDropdown.Width = Width;
        textFlagsInput.Width = Width;
        alignmentEnumDropdown.Width = Width;
        fontSizeInput.Width = Width;
    }

    private void SetAllChildVisibility(bool isVisible)
    {
        headerLabel.IsVisible = isVisible;
        fontTypeEnumDropdown.IsVisible = isVisible;
        textFlagsInput.IsVisible = isVisible;
        alignmentEnumDropdown.IsVisible = isVisible;
        fontSizeInput.IsVisible = isVisible;
    }
}
