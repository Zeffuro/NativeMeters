using System;
using System.Globalization;

namespace NativeMeters.Tags.Formatting;

public class NumericFormatter : IValueFormatter
{
    public bool CanFormat(object value)
    {
        return value is double or long or int ||
               (value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
    }

    public string Format(object value, string subKey, string format, int? precision)
    {
        var num = Convert.ToDouble(value);

        if (format.Equals("k", StringComparison.OrdinalIgnoreCase))
        {
            if (Math.Abs(num) < 1000.0)
                return num.ToString($"N{precision ?? 0}", CultureInfo.InvariantCulture);

            num /= 1000.0;
            return num.ToString($"F{precision ?? 0}", CultureInfo.InvariantCulture) + "k";
        }

        if (format.Equals("m", StringComparison.OrdinalIgnoreCase))
        {
            if (Math.Abs(num) < 1000.0)
                return num.ToString($"N{precision ?? 0}", CultureInfo.InvariantCulture);
            if (Math.Abs(num) < 1_000_000.0)
            {
                num /= 1000.0;
                return num.ToString($"F{precision ?? 0}", CultureInfo.InvariantCulture) + "k";
            }

            num /= 1000000.0;
            return num.ToString($"F{precision ?? 0}", CultureInfo.InvariantCulture) + "m";
        }

        var style = format.Equals("r", StringComparison.OrdinalIgnoreCase) ? "F" : "N";
        return num.ToString(style + (precision ?? 0), CultureInfo.InvariantCulture);
    }
}
