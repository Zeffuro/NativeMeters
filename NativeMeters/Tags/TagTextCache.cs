using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NativeMeters.Tags;

internal sealed class TagTextCache
{
    private List<ParsedTag>? template;
    private object?[] values = [];
    private string?[] formatted = [];
    private CultureInfo?[] cultures = [];
    private bool[] reusable = [];
    private bool isEncounter;
    private string? output;

    public string Process(List<ParsedTag> segments, object data)
    {
        var encounter = data is Models.Encounter;
        var canReuse = ReferenceEquals(template, segments) && encounter == isEncounter;
        if (!ReferenceEquals(template, segments))
        {
            template = segments;
            values = new object?[segments.Count];
            formatted = new string?[segments.Count];
            cultures = new CultureInfo?[segments.Count];
            reusable = new bool[segments.Count];
        }
        if (!canReuse) Array.Clear(reusable);

        isEncounter = encounter;
        var previousOutput = output;
        output = null;
        var changed = !canReuse || previousOutput == null;
        for (var index = 0; index < segments.Count; index++)
        {
            var segment = segments[index];
            if (segment.IsLiteral) continue;

            var value = TagEngine.GetTagValue(segment, data);
            var culture = CultureInfo.CurrentCulture;
            if (canReuse && reusable[index] && ReferenceEquals(cultures[index], culture)
                && culture.IsReadOnly && SameValue(values[index], value)) continue;

            var text = TagEngine.FormatTag(segment, value, data);
            changed |= formatted[index] != text;
            formatted[index] = text;
            reusable[index] = IsCacheable(value);
            values[index] = reusable[index] ? value : null;
            cultures[index] = culture;
        }

        if (!changed) return output = previousOutput!;
        if (segments.Count == 1)
            return output = segments[0].IsLiteral ? segments[0].LiteralText : formatted[0]!;

        var builder = new StringBuilder();
        for (var index = 0; index < segments.Count; index++)
            builder.Append(segments[index].IsLiteral ? segments[index].LiteralText : formatted[index]);
        return output = builder.ToString();
    }

    private static bool IsCacheable(object? value)
        => value is null or string or int or long or double or TimeSpan;

    private static bool SameValue(object? previous, object? current)
    {
        if (!IsCacheable(current)) return false;
        if (previous is double left && current is double right)
            return BitConverter.DoubleToInt64Bits(left) == BitConverter.DoubleToInt64Bits(right);
        return Equals(previous, current);
    }
}
