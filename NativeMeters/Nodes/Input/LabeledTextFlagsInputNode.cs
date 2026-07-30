using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public sealed class LabeledTextFlagsInputNode : LabeledControlRowNode<TextButtonNode>
{
    private TextFlags value;

    public LabeledTextFlagsInputNode() : base(new TextButtonNode())
    {
        MaximumControlWidth = 360.0f;
        ControlNode.LabelNode.TextFlags = TextFlags.Ellipsis;
        ControlNode.OnClick = () =>
        {
            if (!IsEnabled)
                return;

            System.TextFlagsPickerAddon.OpenFor(Value, updatedValue =>
            {
                Value = updatedValue;
                OnValueChanged?.Invoke(updatedValue);
            });
        };
    }

    public TextFlags Value
    {
        get => value;
        set
        {
            this.value = value;
            ControlNode.String = TextFlagOptions.GetSummary(value);
        }
    }

    public bool IsEnabled
    {
        get => ControlNode.IsEnabled;
        set
        {
            ControlNode.IsEnabled = value;
            var alpha = value ? 1.0f : 0.45f;
            Alpha = alpha;
            ControlNode.Alpha = alpha;
        }
    }

    public Action<TextFlags>? OnValueChanged { get; set; }
}

internal static class TextFlagOptions
{
    public static IReadOnlyList<TextFlags> SelectableFlags { get; } = Enum.GetValues<TextFlags>()
        .Where(flag => Convert.ToUInt64(flag) != 0)
        .Where(flag => IsSingleFlag(Convert.ToUInt64(flag)))
        .DistinctBy(flag => Convert.ToUInt64(flag))
        .ToList();

    public static string GetLabel(TextFlags flag)
        => SplitPascalCase(flag.ToString());

    public static string GetSummary(TextFlags flags)
    {
        var selected = SelectableFlags
            .Where(flag => flags.HasFlag(flag))
            .Select(GetLabel)
            .ToList();

        return selected.Count == 0
            ? "None"
            : string.Join(", ", selected);
    }

    private static bool IsSingleFlag(ulong value)
        => (value & (value - 1)) == 0;

    private static string SplitPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var builder = new StringBuilder(value.Length + 8);
        builder.Append(value[0]);

        for (var index = 1; index < value.Length; index++)
        {
            var current = value[index];
            var previous = value[index - 1];

            if (char.IsUpper(current) && !char.IsWhiteSpace(previous))
                builder.Append(' ');

            builder.Append(current);
        }

        return builder.ToString();
    }
}
