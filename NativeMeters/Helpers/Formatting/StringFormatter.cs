using System;

namespace NativeMeters.Helpers.Formatting;

public class StringFormatter : IValueFormatter
{
    public bool CanFormat(object value) => value is string;

    public string Format(object value, string subKey, string format, int? precision)
    {
        var s = (string)value;

        if (!string.IsNullOrEmpty(subKey))
        {
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (subKey.Equals("first", StringComparison.OrdinalIgnoreCase))
                s = parts.Length > 0 ? parts[0] : s;
            else if (subKey.Equals("last", StringComparison.OrdinalIgnoreCase))
                s = parts.Length > 1 ? parts[1] : (parts.Length > 0 ? string.Empty : s);
        }

        return precision.HasValue && s.Length > precision.Value ? s[..precision.Value] : s;
    }
}
