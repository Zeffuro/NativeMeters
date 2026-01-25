namespace NativeMeters.Tags.Formatting;

public interface IValueFormatter
{
    bool CanFormat(object value);
    string Format(object value, string subKey, string format, int? precision);
}
