using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Localization;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

public sealed class ComponentVisualsPanel : VerticalListNode
{
    private ComponentSettings? settings;

    private readonly LabelTextNode headerLabel;
    private readonly ComponentEnumDropdownRowNode<ColorMode> colorModeDropdown;
    private readonly ComponentColorInputRowNode textColorInput;
    private readonly ComponentColorInputRowNode outlineColorInput;
    private readonly ComponentCheckboxRowNode backgroundCheckbox;
    private readonly ComponentColorInputRowNode backgroundTextColorInput;
    private readonly ComponentEnumDropdownRowNode<ProgressBarType> barTypeDropdown;
    private readonly ComponentEnumDropdownRowNode<ProgressBarSizing> barSizingDropdown;
    private readonly ComponentEnumDropdownRowNode<ProgressBarColorTreatment> barColorTreatmentDropdown;
    private readonly ComponentCheckboxRowNode barFillRightToLeftCheckbox;
    private readonly ComponentColorInputRowNode barColorInput;
    private readonly ComponentColorInputRowNode barBgColorInput;

    private bool isLoading = false;
    private float? lastChildWidth;

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
            String = Loc.Get("ComponentVisuals"),
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        colorModeDropdown = new ComponentEnumDropdownRowNode<ColorMode>
        {
            ShowStepButtons = true,
            LabelText = Loc.Get("ComponentColoring"),
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<ColorMode>().ToList(),
            OnOptionSelected = val =>
            {
                if (settings == null || isLoading) return;
                settings.ColorMode = val;
                OnSettingsChanged?.Invoke();
            }
        };

        barSizingDropdown = new ComponentEnumDropdownRowNode<ProgressBarSizing>
        {
            ShowStepButtons = true,
            LabelText = Loc.Get("ComponentBarSizing"),
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<ProgressBarSizing>().ToList(),
            OnOptionSelected = val =>
            {
                if (settings == null || isLoading || settings.ProgressBarSizing == val) return;
                settings.ProgressBarSizing = val;
                OnSettingsChanged?.Invoke();
            }
        };

        barTypeDropdown = new ComponentEnumDropdownRowNode<ProgressBarType>
        {
            ShowStepButtons = true,
            LabelText = Loc.Get("ComponentBarType"),
            Size = new Vector2(Width, 28),
            Options = ProgressBarTypeOptions.Ordered(),
            OnOptionSelected = val =>
            {
                if (settings == null || isLoading) return;
                if (settings.ProgressBarType == val) return;
                settings.ProgressBarType = val;
                var wasSizingVisible = barSizingDropdown.IsVisible;
                ApplyVisibility();
                if (wasSizingVisible != barSizingDropdown.IsVisible)
                {
                    RecalculateLayout();
                    OnLayoutChanged?.Invoke();
                }
                OnSettingsChanged?.Invoke();
            }
        };

        barColorTreatmentDropdown = new ComponentEnumDropdownRowNode<ProgressBarColorTreatment>
        {
            ShowStepButtons = true,
            LabelText = Loc.Get("ComponentBarTint"),
            Size = new Vector2(Width, 28),
            Options = Enum.GetValues<ProgressBarColorTreatment>().ToList(),
            OnOptionSelected = val =>
            {
                if (settings == null || isLoading) return;
                settings.ProgressBarColorTreatment = val;
                OnSettingsChanged?.Invoke();
            }
        };

        barFillRightToLeftCheckbox = new ComponentCheckboxRowNode
        {
            String = Loc.Get("ComponentFillRtl"),
            Size = new Vector2(Width, 28),
            OnClick = val =>
            {
                if (settings == null || isLoading) return;
                if (settings.ProgressBarFillRightToLeft == val) return;
                settings.ProgressBarFillRightToLeft = val;
                OnSettingsChanged?.Invoke();
            }
        };

        textColorInput = new ComponentColorInputRowNode
        {
            Label = Loc.Get("ComponentStaticColor"),
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextColor,
            CurrentColor = settings?.TextColor ?? new ComponentSettings().TextColor,
            OnColorConfirmed = textColorCallback,
            OnColorPreviewed = textColorCallback,
            OnColorCanceled = textColorCallback
        };

        outlineColorInput = new ComponentColorInputRowNode
        {
            Label = Loc.Get("ComponentOutlineColor"),
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextOutlineColor,
            CurrentColor = settings?.TextOutlineColor ?? new ComponentSettings().TextOutlineColor,
            OnColorConfirmed = outlineColorCallback,
            OnColorPreviewed = outlineColorCallback,
            OnColorCanceled = outlineColorCallback
        };

        backgroundTextColorInput = new ComponentColorInputRowNode
        {
            Label = Loc.Get("MeterBackgroundColor"),
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().TextBackgroundColor,
            CurrentColor = settings?.TextBackgroundColor ?? new ComponentSettings().TextBackgroundColor,
            OnColorConfirmed = textBgColorCallback,
            OnColorPreviewed = textBgColorCallback,
            OnColorCanceled = textBgColorCallback
        };

        barColorInput = new ComponentColorInputRowNode
        {
            Label = Loc.Get("ComponentBarColor"),
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().BarColor,
            CurrentColor = settings?.BarColor ?? new ComponentSettings().BarColor,
            OnColorConfirmed = barColorCallback,
            OnColorPreviewed = barColorCallback,
            OnColorCanceled = barColorCallback
        };

        barBgColorInput = new ComponentColorInputRowNode
        {
            Label = Loc.Get("ComponentBarBackground"),
            Size = new Vector2(Width, 28),
            DefaultColor = new ComponentSettings().BarBackgroundColor,
            CurrentColor = settings?.BarColor ?? new ComponentSettings().BarBackgroundColor,
            OnColorConfirmed = barBgColorCallback,
            OnColorPreviewed = barBgColorCallback,
            OnColorCanceled = barBgColorCallback
        };

        backgroundCheckbox = new ComponentCheckboxRowNode
        {
            String = Loc.Get("MeterBackground"),
            Size = new Vector2(Width, 28),
            OnClick = val =>
            {
                if (settings == null || isLoading) return;
                settings.ShowBackground = val;
                OnSettingsChanged?.Invoke();
            }
        };

        AddNode([headerLabel, colorModeDropdown, textColorInput, outlineColorInput, barTypeDropdown,
            barSizingDropdown, barColorTreatmentDropdown, barFillRightToLeftCheckbox, barColorInput, barBgColorInput,
            backgroundCheckbox, backgroundTextColorInput]);
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            var changed = base.IsVisible != value;
            base.IsVisible = value;

            if (value)
            {
                ApplyVisibility();
            }
            else
            {
                SetAllChildVisibility(false);
            }

            if (changed)
            {
                RecalculateLayout();
            }
        }
    }

    public void LoadSettings(ComponentSettings componentSettings)
    {
        isLoading = true;
        settings = componentSettings;

        var isText = settings.Type == MeterComponentType.Text;
        var isBar = settings.Type == MeterComponentType.ProgressBar;
        var isBg = settings.Type == MeterComponentType.Background;
        var wasVisible = IsVisible;
        var wasColorModeVisible = colorModeDropdown.IsVisible;
        var wasTextColorVisible = textColorInput.IsVisible;
        var wasOutlineColorVisible = outlineColorInput.IsVisible;
        var wasBackgroundCheckboxVisible = backgroundCheckbox.IsVisible;
        var wasBackgroundTextColorVisible = backgroundTextColorInput.IsVisible;
        var wasBarTypeVisible = barTypeDropdown.IsVisible;
        var wasBarSizingVisible = barSizingDropdown.IsVisible;
        var wasBarColorTreatmentVisible = barColorTreatmentDropdown.IsVisible;
        var wasBarFillRightToLeftVisible = barFillRightToLeftCheckbox.IsVisible;
        var wasBarColorVisible = barColorInput.IsVisible;
        var wasBarBgColorVisible = barBgColorInput.IsVisible;

        IsVisible = isText || isBar || isBg;

        if (!IsVisible)
        {
            isLoading = false;
            if (wasVisible != IsVisible) OnLayoutChanged?.Invoke();
            return;
        }

        ApplyVisibility();

        backgroundTextColorInput.Label = isBar ? Loc.Get("ComponentBarBackground") : Loc.Get("MeterBackgroundColor");
        textColorInput.Label = isBg ? Loc.Get("ComponentPlateColor") : Loc.Get("ComponentStaticColor");

        colorModeDropdown.SelectedOption = settings.ColorMode;
        textColorInput.CurrentColor = settings.TextColor;
        outlineColorInput.CurrentColor = settings.TextOutlineColor;
        backgroundCheckbox.IsChecked = settings.ShowBackground;
        backgroundTextColorInput.CurrentColor = settings.TextBackgroundColor;
        barTypeDropdown.SelectedOption = settings.ProgressBarType;
        barSizingDropdown.SelectedOption = settings.ProgressBarSizing;
        barColorTreatmentDropdown.SelectedOption = settings.ProgressBarColorTreatment;
        barFillRightToLeftCheckbox.IsChecked = settings.ProgressBarFillRightToLeft;
        barColorInput.CurrentColor = settings.BarColor;
        barBgColorInput.CurrentColor = settings.BarBackgroundColor;

        isLoading = false;
        RecalculateLayout();

        if (wasVisible != IsVisible
            || wasColorModeVisible != colorModeDropdown.IsVisible
            || wasTextColorVisible != textColorInput.IsVisible
            || wasOutlineColorVisible != outlineColorInput.IsVisible
            || wasBackgroundCheckboxVisible != backgroundCheckbox.IsVisible
            || wasBackgroundTextColorVisible != backgroundTextColorInput.IsVisible
            || wasBarTypeVisible != barTypeDropdown.IsVisible
            || wasBarSizingVisible != barSizingDropdown.IsVisible
            || wasBarColorTreatmentVisible != barColorTreatmentDropdown.IsVisible
            || wasBarFillRightToLeftVisible != barFillRightToLeftCheckbox.IsVisible
            || wasBarColorVisible != barColorInput.IsVisible
            || wasBarBgColorVisible != barBgColorInput.IsVisible)
        {
            OnLayoutChanged?.Invoke();
        }
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

    private void ApplyVisibility()
    {
        var isText = settings?.Type == MeterComponentType.Text;
        var isBar = settings?.Type == MeterComponentType.ProgressBar;
        var isBg = settings?.Type == MeterComponentType.Background;

        headerLabel.IsVisible = true;
        colorModeDropdown.IsVisible = isText || isBar;
        textColorInput.IsVisible = isText || isBg;
        outlineColorInput.IsVisible = isText;
        SetBackgroundCheckboxVisible(isText);
        backgroundTextColorInput.IsVisible = isText;
        barTypeDropdown.IsVisible = isBar;
        barSizingDropdown.IsVisible = isBar && ProgressBarTypeOptions.SupportsPreservingEnds(settings!.ProgressBarType);
        barColorTreatmentDropdown.IsVisible = isBar;
        barFillRightToLeftCheckbox.IsVisible = isBar;
        barColorInput.IsVisible = isBar;
        barBgColorInput.IsVisible = isBar;
    }

    private void SetAllChildVisibility(bool isVisible)
    {
        headerLabel.IsVisible = isVisible;
        colorModeDropdown.IsVisible = isVisible;
        textColorInput.IsVisible = isVisible;
        outlineColorInput.IsVisible = isVisible;
        SetBackgroundCheckboxVisible(isVisible);
        backgroundTextColorInput.IsVisible = isVisible;
        barTypeDropdown.IsVisible = isVisible;
        barSizingDropdown.IsVisible = isVisible;
        barColorTreatmentDropdown.IsVisible = isVisible;
        barFillRightToLeftCheckbox.IsVisible = isVisible;
        barColorInput.IsVisible = isVisible;
        barBgColorInput.IsVisible = isVisible;
    }

    private void SetBackgroundCheckboxVisible(bool isVisible)
    {
        backgroundCheckbox.IsVisible = isVisible;
    }

    protected override void OnSizeChanged()
    {
        if (lastChildWidth == Width) return;
        lastChildWidth = Width;
        base.OnSizeChanged();
        headerLabel.Width = Width;
        colorModeDropdown.Width = Width;
        textColorInput.Width = Width;
        outlineColorInput.Width = Width;
        backgroundCheckbox.Width = Width;
        backgroundTextColorInput.Width = Width;
        barTypeDropdown.Width = Width;
        barSizingDropdown.Width = Width;
        barColorTreatmentDropdown.Width = Width;
        barFillRightToLeftCheckbox.Width = Width;
        barColorInput.Width = Width;
        barBgColorInput.Width = Width;
    }
}
