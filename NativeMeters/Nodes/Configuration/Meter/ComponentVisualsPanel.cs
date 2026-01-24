using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentVisualsPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly LabeledEnumDropdownNode<ColorMode> colorModeDropdown;
    private readonly ColorInputRow textColorInput;
    private readonly ColorInputRow outlineColorInput;
    private readonly CheckboxNode backgroundCheckbox;
    private readonly ColorInputRow backgroundTextColorInput;
    private readonly ColorInputRow barColorInput;
    private readonly ColorInputRow barBgColorInput;

    private bool isLoading = false;

    public Action? OnSettingsChanged { get; set; }
    public Action? OnLayoutChanged { get; set; }

    public ComponentVisualsPanel()
    {
        FitContents = true;
        ItemSpacing = 4.0f;

        var textBgColorCallback = CreateColorCallback(color => settings?.TextBackgroundColor = color);
        var textColorCallback = CreateColorCallback(color => settings?.TextColor = color);
        var outlineColorCallback = CreateColorCallback(color => settings?.TextOutlineColor = color);
        var barColorCallback = CreateColorCallback(color => settings?.BarColor = color);
        var barBgColorCallback = CreateColorCallback(color => settings?.BarBackgroundColor = color);

        headerLabel = new LabelTextNode
        {
            String = "Visuals",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        colorModeDropdown = new LabeledEnumDropdownNode<ColorMode>
        {
            LabelText = "Coloring Mode:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<ColorMode>().ToList(),
            OnOptionSelected = val =>
            {
                if (settings == null || isLoading) return;
                settings.ColorMode = val;
                OnSettingsChanged?.Invoke();
            }
        };

        textColorInput = new ColorInputRow
        {
            Label = "Static Color",
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextColor,
            CurrentColor = settings?.TextColor ?? new ComponentSettings().TextColor,
            OnColorConfirmed = textColorCallback,
            OnColorPreviewed = textColorCallback,
            OnColorCanceled = textColorCallback
        };

        outlineColorInput = new ColorInputRow
        {
            Label = "Outline Color",
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextOutlineColor,
            CurrentColor = settings?.TextOutlineColor ?? new ComponentSettings().TextOutlineColor,
            OnColorConfirmed = outlineColorCallback,
            OnColorPreviewed = outlineColorCallback,
            OnColorCanceled = outlineColorCallback
        };

        backgroundTextColorInput = new ColorInputRow
        {
            Label = "Background Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextBackgroundColor,
            CurrentColor = settings?.TextBackgroundColor ?? new ComponentSettings().TextBackgroundColor,
            OnColorConfirmed = textBgColorCallback,
            OnColorPreviewed = textBgColorCallback,
            OnColorCanceled = textBgColorCallback
        };

        barColorInput = new ColorInputRow
        {
            Label = "Bar Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().BarColor,
            CurrentColor = settings?.BarColor ?? new ComponentSettings().BarColor,
            OnColorConfirmed = barColorCallback,
            OnColorPreviewed = barColorCallback,
            OnColorCanceled = barColorCallback
        };

        barBgColorInput = new ColorInputRow
        {
            Label = "Bar Background Color: ",
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().BarBackgroundColor,
            CurrentColor = settings?.BarColor ?? new ComponentSettings().BarBackgroundColor,
            OnColorConfirmed = barBgColorCallback,
            OnColorPreviewed = barBgColorCallback,
            OnColorCanceled = barBgColorCallback
        };

        backgroundCheckbox = new CheckboxNode
        {
            String = "Show BG",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings == null || isLoading) return;
                settings.ShowBackground = val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([headerLabel, colorModeDropdown, textColorInput, outlineColorInput, barColorInput, barBgColorInput, backgroundCheckbox, backgroundTextColorInput]);
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        var isText = settings.Type == MeterComponentType.Text;
        var isBar = settings.Type == MeterComponentType.ProgressBar;
        var isBg = settings.Type == MeterComponentType.Background;
        var wasVisible = IsVisible;

        IsVisible = isText || isBar || isBg;

        if (!IsVisible)
        {
            if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
            return;
        }

        colorModeDropdown.IsVisible = isText || isBar;
        textColorInput.IsVisible = isText || isBg;
        outlineColorInput.IsVisible = isText;
        backgroundCheckbox.IsVisible = isText;
        backgroundTextColorInput.IsVisible = isText;
        barColorInput.IsVisible = isBar;
        barBgColorInput.IsVisible = isBar;

        backgroundTextColorInput.Label = isBar ? "Bar BG Color: " : "Background Color: ";
        textColorInput.Label = isBg ? "Plate Color: " : "Static Color: ";

        colorModeDropdown.SelectedOption = settings.ColorMode;
        textColorInput.CurrentColor = settings.TextColor;
        outlineColorInput.CurrentColor = settings.TextOutlineColor;
        backgroundCheckbox.IsChecked = settings.ShowBackground;
        backgroundTextColorInput.CurrentColor = settings.TextBackgroundColor;
        barColorInput.CurrentColor = settings.BarColor;
        barBgColorInput.CurrentColor = settings.BarBackgroundColor;

        isLoading = false;
        RecalculateLayout();
        if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
    }

    private Action<Vector4> CreateColorCallback(Action<Vector4> setter)
    {
        return color =>
        {
            if (settings == null || isLoading) return;
            setter(color);
            OnSettingsChanged?.Invoke();
        };
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        headerLabel.Width = Width;
        colorModeDropdown.Width = Width;
        textColorInput.Width = Width;
        outlineColorInput.Width = Width;
        backgroundCheckbox.Width = Width;
        backgroundTextColorInput.Width = Width;
        barColorInput.Width = Width;
        barBgColorInput.Width = Width;
    }
}
