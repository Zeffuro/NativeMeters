using System.Collections.Generic;

namespace NativeMeters.Tags.Formatting;

public class ValueFormatterChain
{
    private readonly List<IValueFormatter> formatters =
    [
        new ClassJobFormatter(),
        new TimeSpanFormatter(),
        new NumericFormatter(),
        new StringFormatter()
    ];

    public string Format(object value, string subKey, string format, int? precision)
    {
        foreach (var formatter in formatters)
        {
            if (formatter.CanFormat(value))
                return formatter.Format(value, subKey, format, precision);
        }

        return value?.ToString() ?? string.Empty;
    }
}
