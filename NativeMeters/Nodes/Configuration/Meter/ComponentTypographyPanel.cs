using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentTypographyPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly LabeledDropdownNode fontTypeDropdown;
    private readonly LabeledDropdownNode textFlagsDropdown;
    private readonly LabeledDropdownNode alignmentDropdown;
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

        fontTypeDropdown = new LabeledDropdownNode
        {
            LabelText = "Font:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<FontType>().ToList(),
            OnOptionSelected = val =>
            {
                if (Enum.TryParse<FontType>(val, out var font) && settings != null || !isLoading)
                {
                    settings.FontType = font;
                    OnSettingsChanged?.Invoke();
                }
            }
        };

        textFlagsDropdown = new LabeledDropdownNode
        {
            LabelText = "Style:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<TextFlags>().ToList(),
            OnOptionSelected = val =>
            {
                if (Enum.TryParse<TextFlags>(val, out var flags) && settings != null || !isLoading)
                {
                    settings.TextFlags = flags;
                    OnSettingsChanged?.Invoke();
                }
            }
        };

        alignmentDropdown = new LabeledDropdownNode
        {
            LabelText = "Alignment:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<AlignmentType>().ToList(),
            OnOptionSelected = val =>
            {
                if (Enum.TryParse<AlignmentType>(val, out var align) && settings != null || !isLoading)
                {
                    settings.AlignmentType = align;
                    OnSettingsChanged?.Invoke();
                }
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

        AddNode([headerLabel, fontTypeDropdown, alignmentDropdown, fontSizeInput, textFlagsDropdown]);
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

        fontTypeDropdown.SelectedOption = settings.FontType.ToString();
        textFlagsDropdown.SelectedOption = settings.TextFlags.ToString();
        alignmentDropdown.SelectedOption = settings.AlignmentType.ToString();
        fontSizeInput.Value = (int)settings.FontSize;

        isLoading = false;
        RecalculateLayout();
        if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        headerLabel.Width = Width;
        fontTypeDropdown.Width = Width;
        textFlagsDropdown.Width = Width;
        alignmentDropdown.Width = Width;
        fontSizeInput.Width = Width;
    }
}
