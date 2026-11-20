using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Nodes.Configuration;

namespace NativeMeters.Nodes.Input;

public abstract class LabeledDropdownRowNode<T, TControl> : LabeledControlRowNode<TControl>
    where TControl : DropDownNode<T>
{
    private const float ArrowSize = 18.0f;
    private const float ArrowGap = 4.0f;
    private const float MinimumLabelWidth = 92.0f;
    private const float MinimumDropdownWidth = 96.0f;

    private (CircleButtonNode Previous, CircleButtonNode Next)? stepButtons;
    private bool stepButtonsFit;

    protected LabeledDropdownRowNode(TControl controlNode) : base(controlNode) { }

    public bool ShowStepButtons
    {
        get => stepButtons.HasValue;
        init
        {
            if (!value) return;

            var previous = new CircleButtonNode
            {
                Icon = CircleButtonIcon.LeftArrow,
                TextTooltip = Loc.Get("CommonPreviousOption"),
                OnClick = () => Step(-1),
                IsEnabled = IsEnabled,
            };
            var next = new CircleButtonNode
            {
                Icon = CircleButtonIcon.RightArrow,
                TextTooltip = Loc.Get("CommonNextOption"),
                OnClick = () => Step(1),
                IsEnabled = IsEnabled,
            };
            previous.AttachNode(this);
            next.AttachNode(this);
            stepButtons = (previous, next);
            OnSizeChanged();
        }
    }

    public Action<T>? OnOptionSelected
    {
        get => ControlNode.OnOptionSelected;
        set => ControlNode.OnOptionSelected = value;
    }

    public int MaxListOptions
    {
        get => ControlNode.MaxListOptions;
        set => ControlNode.MaxListOptions = value;
    }

    public bool IsEnabled
    {
        get => ControlNode.IsEnabled;
        set
        {
            if (ControlNode.IsEnabled == value) return;
            if (!value) ControlNode.Collapse(playSoundEffect: false);
            ControlNode.IsEnabled = value;
            if (stepButtons is { } buttons)
                buttons.Previous.IsEnabled = buttons.Next.IsEnabled = value;

            Alpha = value ? 1.0f : 0.45f;
        }
    }

    public required List<T> Options
    {
        get => ControlNode.Options;
        set
        {
            ControlNode.Options = value;
            if (stepButtons.HasValue) OnSizeChanged();
        }
    }

    public override IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        if (stepButtons is { } buttons && buttons.Previous.IsVisible)
        {
            yield return ConfigurationNavigationTarget.From(buttons.Previous);
            yield return ConfigurationNavigationTarget.From(ControlNode);
            yield return ConfigurationNavigationTarget.From(buttons.Next);
            yield break;
        }

        foreach (var target in base.GetNavigationTargets())
            yield return target;
    }

    protected override void LayoutRow(float labelWidth, float controlX, float controlWidth)
    {
        const float arrowSpace = ArrowSize + ArrowGap;
        if (stepButtons is { } buttons)
        {
            var previousX = controlX - arrowSpace;
            stepButtonsFit = previousX >= MinimumLabelWidth
                && controlWidth >= MinimumDropdownWidth + arrowSpace
                && ControlNode.Options.Count > 1;
            UpdateStepButtonsVisibility();

            if (stepButtonsFit)
            {
                var y = (Height - ArrowSize) / 2;
                buttons.Previous.Position = new Vector2(previousX, y);
                buttons.Next.Position = new Vector2(controlX + controlWidth - ArrowSize, y);
                ConfigurationLayout.SetSize(buttons.Previous, new Vector2(ArrowSize));
                ConfigurationLayout.SetSize(buttons.Next, new Vector2(ArrowSize));
                labelWidth = Math.Min(labelWidth, previousX);
                controlWidth -= arrowSpace;
            }
        }

        base.LayoutRow(labelWidth, controlX, controlWidth);
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
        UpdateStepButtonsVisibility();
    }

    private void UpdateStepButtonsVisibility()
    {
        if (stepButtons is not { } buttons) return;
        buttons.Previous.IsVisible = buttons.Next.IsVisible = IsVisible && stepButtonsFit;
    }

    private void Step(int direction)
    {
        var options = ControlNode.Options;
        if (!IsEnabled || !IsVisible || !stepButtonsFit || options.Count < 2) return;

        ControlNode.Collapse(playSoundEffect: false);
        var selected = ControlNode.SelectedOption;
        var index = selected is null ? -1 : options.IndexOf(selected);
        if (index < 0) index = direction > 0 ? options.Count - 1 : 0;

        var option = options[(index + direction + options.Count) % options.Count];
        ControlNode.SelectedOption = option;
        OnOptionSelected?.Invoke(option);
    }
}
