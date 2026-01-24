using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;
using Serilog.Parsing;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentTypographyPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly LabeledEnumDropdownNode<FontType> fontTypeEnumDropdown;
    private readonly LabeledEnumDropdownNode<TextFlags> textFlagsEnumDropdown;
    private readonly LabeledEnumDropdownNode<AlignmentType> alignmentEnumDropdown;
    private readonly LabeledNumericInputNode fontSizeInput;

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

        fontTypeEnumDropdown = new LabeledEnumDropdownNode<FontType>
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

        textFlagsEnumDropdown = new LabeledEnumDropdownNode<TextFlags>
        {
            LabelText = "Style:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<TextFlags>().ToList(),
            OnOptionSelected = val =>
            {
                settings?.TextFlags = val;
                OnSettingsChanged?.Invoke();
            }
        };

        alignmentEnumDropdown = new LabeledEnumDropdownNode<AlignmentType>
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

        fontSizeInput = new LabeledNumericInputNode
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

        AddNode([headerLabel, fontTypeEnumDropdown, alignmentEnumDropdown, fontSizeInput, textFlagsEnumDropdown]);
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        var isText = settings.Type == MeterComponentType.Text;
        var wasVisible = IsVisible;

        IsVisible = isText;

        if (!isText)
        {
            if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
            return;
        }

        fontTypeEnumDropdown.SelectedOption = settings.FontType;
        textFlagsEnumDropdown.SelectedOption = settings.TextFlags;
        alignmentEnumDropdown.SelectedOption = settings.AlignmentType;
        fontSizeInput.Value = (int)settings.FontSize;

        isLoading = false;
        RecalculateLayout();
        if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        headerLabel.Width = Width;
        fontTypeEnumDropdown.Width = Width;
        textFlagsEnumDropdown.Width = Width;
        alignmentEnumDropdown.Width = Width;
        fontSizeInput.Width = Width;
    }
}
