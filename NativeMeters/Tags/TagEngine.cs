using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NativeMeters.Models;
using NativeMeters.Tags.Formatting;
using NativeMeters.Tags.Reflection;

namespace NativeMeters.Tags;

public static class TagEngine
{
    private static readonly Regex TagRegex = new(
        @"\[(?<key>[^:\]\._]+)(?:_(?<subKey>[^:\]\.]+))?(?::(?<format>[^\]\.]+))?(?:\.(?<precision>\d+))?\]",
        RegexOptions.Compiled);

    private static readonly ValueFormatterChain FormatterChain = new();

    private static readonly Dictionary<string, List<ParsedTag>> ParseCache = new(StringComparer.Ordinal);

    public static string Process(string input, object data)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (!ParseCache.TryGetValue(input, out var segments))
        {
            segments = ParseTemplate(input);
            ParseCache[input] = segments;
        }

        if (segments.Count == 1 && segments[0].IsLiteral)
            return segments[0].LiteralText;

        if (segments.Count == 1 && !segments[0].IsLiteral)
            return ResolveTag(segments[0], data);

        var sb = new StringBuilder();
        foreach (var segment in segments)
        {
            sb.Append(segment.IsLiteral ? segment.LiteralText : ResolveTag(segment, data));
        }
        return sb.ToString();
    }

    public static void ClearCache()
    {
        ParseCache.Clear();
    }

    private static List<ParsedTag> ParseTemplate(string input)
    {
        var segments = new List<ParsedTag>();
        var matches = TagRegex.Matches(input);

        int lastEnd = 0;
        foreach (Match match in matches)
        {
            if (match.Index > lastEnd)
            {
                segments.Add(ParsedTag.Literal(input[lastEnd..match.Index]));
            }

            var key = match.Groups["key"].Value;
            var subKey = match.Groups["subKey"].Value;
            var format = match.Groups["format"].Value;
            var precisionStr = match.Groups["precision"].Value;
            int? precision = string.IsNullOrEmpty(precisionStr) ? null : int.Parse(precisionStr);

            segments.Add(ParsedTag.Tag(key, subKey, format, precision, match.Value));

            lastEnd = match.Index + match.Length;
        }

        if (lastEnd < input.Length)
        {
            segments.Add(ParsedTag.Literal(input[lastEnd..]));
        }

        if (segments.Count == 0)
        {
            segments.Add(ParsedTag.Literal(input));
        }

        return segments;
    }

    private static string ResolveTag(ParsedTag tag, object data)
    {
        var rawValue = PropertyAccessor.GetValue(tag.Key, data);
        if (rawValue == null)
            return tag.OriginalText;

        rawValue = PreProcessSubKey(rawValue, tag.SubKey, data);

        return FormatterChain.Format(rawValue, tag.SubKey, tag.Format, tag.Precision);
    }

    private static object PreProcessSubKey(object value, string subKey, object data)
    {
        if (string.IsNullOrEmpty(subKey))
            return value;

        if (!subKey.Equals("skill", StringComparison.OrdinalIgnoreCase) &&
            !subKey.Equals("val", StringComparison.OrdinalIgnoreCase))
            return value;

        var str = value.ToString() ?? "";

        // ACT Format examples:
        // Combatant: "Skill Name-12345"
        // Encounter: "Player Name-Skill Name-12345"

        int lastDash = str.LastIndexOf('-');
        if (lastDash == -1) return value;

        if (subKey.Equals("val", StringComparison.OrdinalIgnoreCase))
        {
            return str.Substring(lastDash + 1);
        }

        if (subKey.Equals("skill", StringComparison.OrdinalIgnoreCase))
        {
            var prefix = str.Substring(0, lastDash);

            if (data is Encounter)
            {
                int firstDash = prefix.IndexOf('-');
                if (firstDash != -1)
                    return prefix.Substring(firstDash + 1);
            }

            return prefix;
        }

        return value;
    }
}
