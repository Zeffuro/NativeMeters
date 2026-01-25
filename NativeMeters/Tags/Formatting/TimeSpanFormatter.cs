using System;

namespace NativeMeters.Tags.Formatting;

public class TimeSpanFormatter : IValueFormatter
{
    public bool CanFormat(object value) => value is TimeSpan;

    public string Format(object value, string subKey, string format, int? precision)
    {
        var timeSpan = (TimeSpan)value;
        return timeSpan.ToString(precision == 1 ? @"mm\:ss" : @"m\:ss");
    }
}
