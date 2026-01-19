using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Color;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentVisualsPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly CheckboxNode jobColorCheckbox;
    private readonly ColorInputRow textColorInput;
    private readonly CheckboxNode outlineCheckbox;
    private readonly ColorInputRow outlineColorInput;
    private readonly CheckboxNode backgroundCheckbox;

    public Action? OnSettingsChanged { get; set; }
    public Action? OnLayoutChanged { get; set; }

    public ComponentVisualsPanel()
    {
        FitContents = true;
        ItemSpacing = 4.0f;

        headerLabel = new LabelTextNode
        {
            String = "Visuals",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        jobColorCheckbox = new CheckboxNode
        {
            String = "Use Job Color",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings == null) return;
                settings.UseJobColor = val;
                OnSettingsChanged?.Invoke();
            }
        };

        textColorInput = new ColorInputRow
        {
            Label = "Static Color",
            Size = new Vector2(Width, 28),
            DefaultColor = Vector4.One,
            CurrentColor = Vector4.One,
            OnColorConfirmed = color =>
            {
                if (settings == null) return;
                settings.TextColor = color;
                OnSettingsChanged?.Invoke();
            }
        };

        outlineCheckbox = new CheckboxNode
        {
            String = "Show Outline",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings == null) return;
                settings.ShowOutline = val;
                outlineColorInput.IsVisible = val;
                RecalculateLayout();
                OnLayoutChanged?.Invoke();
                OnSettingsChanged?.Invoke();
            }
        };

        outlineColorInput = new ColorInputRow
        {
            Label = "Outline Color",
            Size = new Vector2(Width, 28),
            DefaultColor = new Vector4(0, 0, 0, 1),
            CurrentColor = new Vector4(0, 0, 0, 1),
            OnColorConfirmed = color =>
            {
                if (settings == null) return;
                settings.TextOutlineColor = color;
                OnSettingsChanged?.Invoke();
            }
        };

        backgroundCheckbox = new CheckboxNode
        {
            String = "Show BG",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings == null) return;
                settings.ShowBackground = val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([headerLabel, jobColorCheckbox, textColorInput, outlineCheckbox, outlineColorInput, backgroundCheckbox]);
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        settings = componentSettings;

        var isText = settings.Type == MeterComponentType.Text;
        var isBar = settings.Type == MeterComponentType.ProgressBar;
        var isBG = settings.Type == MeterComponentType.Background;
        var wasVisible = IsVisible;

        IsVisible = isText || isBar || isBG;

        if (!IsVisible)
        {
            if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
            return;
        }

        jobColorCheckbox.IsVisible = isText || isBar;
        textColorInput.IsVisible = isText || isBG;
        outlineCheckbox.IsVisible = isText;
        outlineColorInput.IsVisible = isText && settings.ShowOutline;
        backgroundCheckbox.IsVisible = isText;

        textColorInput.Label = isBG ? "Plate Color: " : "Static Color: ";

        jobColorCheckbox.IsChecked = settings.UseJobColor;
        textColorInput.CurrentColor = settings.TextColor;
        outlineCheckbox.IsChecked = settings.ShowOutline;
        outlineColorInput.CurrentColor = settings.TextOutlineColor;
        backgroundCheckbox.IsChecked = settings.ShowBackground;

        RecalculateLayout();
        if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        headerLabel.Width = Width;
        jobColorCheckbox.Width = Width;
        textColorInput.Width = Width;
        outlineCheckbox.Width = Width;
        outlineColorInput.Width = Width;
        backgroundCheckbox.Width = Width;
    }
}
