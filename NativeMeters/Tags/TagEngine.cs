using System;
using System.Text.RegularExpressions;
using NativeMeters.Tags.Formatting;
using NativeMeters.Tags.Reflection;

namespace NativeMeters.Tags;

public static class TagEngine
{
    private static readonly Regex TagRegex = new(
        @"\[(?<key>[^:\]\._]+)(?:_(?<subKey>[^:\]\.]+))?(?::(?<format>[^\]\.]+))?(?:\.(?<precision>\d+))?\]",
        RegexOptions.Compiled);

    private static readonly ValueFormatterChain FormatterChain = new();

    public static string Process(string input, object data)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : TagRegex.Replace(input, match => ProcessMatch(match, data));
    }

    private static string ProcessMatch(Match match, object data)
    {
        var key = match.Groups["key"].Value;
        var subKey = match.Groups["subKey"].Value;
        var format = match.Groups["format"].Value;
        var precisionStr = match.Groups["precision"].Value;
        int? precision = string.IsNullOrEmpty(precisionStr) ? null : int.Parse(precisionStr);

        var rawValue = PropertyAccessor.GetValue(key, data);
        if (rawValue == null) return match.Value;

        rawValue = PreProcessSubKey(rawValue, subKey);

        return FormatterChain.Format(rawValue, subKey, format, precision);
    }

    private static object PreProcessSubKey(object value, string subKey)
    {
        if (!subKey.Equals("skill", StringComparison.OrdinalIgnoreCase) &&
            !subKey.Equals("val", StringComparison.OrdinalIgnoreCase))
            return value;

        var str = value.ToString() ?? "";
        var parts = str.Split('-');

        if (parts.Length >= 3)
        {
            if (subKey.Equals("skill", StringComparison.OrdinalIgnoreCase)) return parts[1];
            if (subKey.Equals("val", StringComparison.OrdinalIgnoreCase)) return parts[2];
        }
        else if (parts.Length == 2 && subKey.Equals("val", StringComparison.OrdinalIgnoreCase))
        {
            return parts[1];
        }

        return value;
    }
}
