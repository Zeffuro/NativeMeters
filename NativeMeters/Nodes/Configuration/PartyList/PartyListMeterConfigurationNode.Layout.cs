using System;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Input;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode
{
    private const float CompactInputControlWidth = 82.0f;
    private const float CompactAxisLabelWidth = 24.0f;
    private const float CompactSizeLabelWidth = 64.0f;

    private static FixedHeightRowNode CreateFixedHeightRow(
        NodeBase child,
        float height = ControlHeight,
        bool fitChildWidth = true)
        => new(child, height, fitChildWidth);

    private static CircleButtonNode CreateFormatHelpButton()
        => new()
        {
            Icon = CircleButtonIcon.QuestionMark,
            Size = new Vector2(FormatButtonSize),
            Y = (ControlHeight - FormatButtonSize) / 2.0f,
            TextTooltip = TagFormatHelp.ComponentTooltip,
        };

    private static CircleButtonNode CreateTagBrowserButton(Action onClick)
        => new()
        {
            Icon = CircleButtonIcon.MagnifyingGlass,
            Size = new Vector2(FormatButtonSize),
            Y = (ControlHeight - FormatButtonSize) / 2.0f,
            OnClick = onClick,
        };

    private LabeledNumericInputNode CreateNumericInput(
        string label,
        int value,
        int min,
        int max,
        Action<int> applyValue)
        => new()
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = label,
            Min = min,
            Max = max,
            Value = value,
            OnValueUpdate = newValue =>
            {
                applyValue(newValue);
                SaveAndUpdate();
            },
        };

    private static HorizontalListNode CreatePairedInputRow()
        => new()
        {
            Size = new Vector2(360, ControlHeight),
            ItemSpacing = 6.0f,
        };

    private LabeledNumericInputNode CreateCompactNumericInput(
        string label,
        int value,
        int min,
        int max,
        Action<int> applyValue)
    {
        var input = CreateNumericInput(label, value, min, max, applyValue);
        input.LabelWidth = label is "X:" or "Y:" ? CompactAxisLabelWidth : CompactSizeLabelWidth;
        input.ControlSpacing = 4.0f;
        input.MaximumControlWidth = CompactInputControlWidth;
        input.Size = new Vector2(GetCompactNumericInputWidth(input), ControlHeight);

        return input;
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        UpdateFormatRowLayouts();
        UpdatePairedInputRowLayouts();
    }

    private void UpdateFormatRowLayouts()
    {
        if (formatRow == null
            || formatInput == null
            || formatHelpButton == null
            || browseTagButton == null
            || raidFormatRow == null
            || raidFormatInput == null
            || raidFormatHelpButton == null
            || raidBrowseTagButton == null)
        {
            return;
        }

        UpdateFormatRowLayout(formatRow, formatInput, formatHelpButton, browseTagButton);
        UpdateFormatRowLayout(raidFormatRow, raidFormatInput, raidFormatHelpButton, raidBrowseTagButton);
    }

    private void UpdatePairedInputRowLayouts()
    {
        if (memberOffsetRow == null
            || memberSizeRow == null
            || barOffsetRow == null
            || barSizeRow == null
            || raidOffsetRow == null
            || raidSizeRow == null)
        {
            return;
        }

        UpdatePairedInputRowLayout(memberOffsetRow, offsetXInput, offsetYInput);
        UpdatePairedInputRowLayout(memberSizeRow, widthInput, heightInput);
        UpdatePairedInputRowLayout(barOffsetRow, barOffsetXInput, barOffsetYInput);
        UpdatePairedInputRowLayout(barSizeRow, barWidthInput, barHeightInput);
        UpdatePairedInputRowLayout(raidOffsetRow, raidOffsetXInput, raidOffsetYInput);
        UpdatePairedInputRowLayout(raidSizeRow, raidWidthInput, raidHeightInput);
    }

    private static void UpdatePairedInputRowLayout(
        HorizontalListNode row,
        LabeledNumericInputNode firstInput,
        LabeledNumericInputNode secondInput)
    {
        row.Height = ControlHeight;

        var availableWidth = Math.Max(0.0f, row.Width - row.ItemSpacing);
        var firstInputWidth = GetCompactNumericInputWidth(firstInput);
        var secondInputWidth = GetCompactNumericInputWidth(secondInput);

        if (firstInputWidth + secondInputWidth > availableWidth)
        {
            firstInputWidth = Math.Max(0.0f, availableWidth / 2.0f);
            secondInputWidth = firstInputWidth;
        }

        firstInput.Size = new Vector2(firstInputWidth, ControlHeight);
        secondInput.Size = new Vector2(secondInputWidth, ControlHeight);
        row.RecalculateLayout();
    }

    private static float GetCompactNumericInputWidth(LabeledNumericInputNode input)
        => input.LabelWidth + input.ControlSpacing + input.MaximumControlWidth;

    private static void UpdateFormatRowLayout(
        HorizontalListNode row,
        LabeledTextInputNode input,
        CircleButtonNode helpButton,
        CircleButtonNode browseButton)
    {
        row.Height = ControlHeight;
        helpButton.Size = new Vector2(FormatButtonSize);
        browseButton.Size = new Vector2(FormatButtonSize);
        helpButton.Y = (ControlHeight - FormatButtonSize) / 2.0f;
        browseButton.Y = (ControlHeight - FormatButtonSize) / 2.0f;

        var rowWidth = Math.Max(0.0f, row.Width);
        var buttonWidth = helpButton.Width + browseButton.Width + row.ItemSpacing * 2.0f;
        var maxInputWidth = Math.Max(0.0f, rowWidth - buttonWidth);
        var preferredInputWidth = input.LabelWidth + input.ControlSpacing + input.MaximumControlWidth;

        input.Size = new Vector2(Math.Min(maxInputWidth, preferredInputWidth), ControlHeight);
        row.RecalculateLayout();
    }

    private sealed class FixedHeightRowNode : ResNode
    {
        private readonly NodeBase child;
        private readonly bool fitChildWidth;

        public FixedHeightRowNode(NodeBase child, float height, bool fitChildWidth)
        {
            this.child = child;
            this.fitChildWidth = fitChildWidth;

            Height = height;
            child.Position = Vector2.Zero;
            child.Height = height;
            child.AttachNode(this);
        }

        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                child.IsVisible = value;
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            child.Position = Vector2.Zero;
            child.Height = Height;

            if (fitChildWidth)
            {
                child.Width = Width;
            }
        }
    }
}
