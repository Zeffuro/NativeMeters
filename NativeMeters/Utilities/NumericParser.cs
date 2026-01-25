using System.Collections.Generic;
using System.Globalization;

namespace NativeMeters.Utilities;

public static class NumericParser
{
    private static readonly Dictionary<char, double> Multipliers = new()
    {
        { 'K', 1_000 },
        { 'M', 1_000_000 },
        { 'B', 1_000_000_000 }
    };

    public static bool TryParse(string? input, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim().ToUpperInvariant();

        var suffix = input[^1];
        var multiplier = Multipliers.GetValueOrDefault(suffix, 1.0);
        var numberPart = multiplier > 1 ? input[..^1] : input;

        if (!double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return false;

        value *= multiplier;
        return true;
    }

    public static string Format(double value, char suffix, int precision = 0)
    {
        var multiplier = Multipliers.GetValueOrDefault(char.ToUpper(suffix), 1.0);
        return (value / multiplier).ToString($"F{precision}", CultureInfo.InvariantCulture) + char.ToLower(suffix);
    }
}
