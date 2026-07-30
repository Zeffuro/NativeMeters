using System;
using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Input;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode
{
    private const float CompactInputControlWidth = 82.0f;
    private const float CompactLabelWidth = 64.0f;

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

    private static LabeledInputPairRowNode CreatePairedInputRow(string label)
        => new()
        {
            Size = new Vector2(360, ControlHeight),
            LabelText = label,
            MaximumControlWidth = float.PositiveInfinity,
        };

    private LabeledNumericInputNode CreateCompactNumericInput(
        string label,
        int value,
        int min,
        int max,
        Action<int> applyValue)
    {
        var input = CreateNumericInput(label, value, min, max, applyValue);
        input.LabelWidth = CompactLabelWidth;
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
            || topMemberFormatRow == null
            || topMemberFormatInput == null
            || topMemberFormatHelpButton == null
            || topMemberBrowseTagButton == null
            || raidFormatRow == null
            || raidFormatInput == null
            || raidFormatHelpButton == null
            || raidBrowseTagButton == null)
        {
            return;
        }

        UpdateFormatRowLayout(formatRow, formatInput, formatHelpButton, browseTagButton);
        UpdateFormatRowLayout(topMemberFormatRow, topMemberFormatInput, topMemberFormatHelpButton, topMemberBrowseTagButton);
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
        LabeledInputPairRowNode row,
        LabeledNumericInputNode firstInput,
        LabeledNumericInputNode secondInput)
    {
        row.Height = ControlHeight;

        var availableWidth = Math.Max(0.0f, row.InputRow.Width - row.InputRow.ItemSpacing);
        var firstInputWidth = GetCompactNumericInputWidth(firstInput);
        var secondInputWidth = GetCompactNumericInputWidth(secondInput);

        if (firstInputWidth + secondInputWidth > availableWidth)
        {
            firstInputWidth = Math.Max(0.0f, availableWidth / 2.0f);
            secondInputWidth = firstInputWidth;
        }

        firstInput.Size = new Vector2(firstInputWidth, ControlHeight);
        secondInput.Size = new Vector2(secondInputWidth, ControlHeight);
        row.InputRow.RecalculateLayout();
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

    private sealed class LabeledInputPairRowNode : LabeledControlRowNode<HorizontalListNode>
    {
        public LabeledInputPairRowNode() : base(new HorizontalListNode
        {
            ItemSpacing = 6.0f,
            FitHeight = true,
        })
        {
        }

        public HorizontalListNode InputRow => ControlNode;

        public void AddInputPair(LabeledNumericInputNode firstInput, LabeledNumericInputNode secondInput)
        {
            ControlNode.AddNode(firstInput);
            ControlNode.AddNode(secondInput);
        }
    }
}
