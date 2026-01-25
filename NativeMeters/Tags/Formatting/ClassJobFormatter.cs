using Lumina.Excel.Sheets;

namespace NativeMeters.Tags.Formatting;

public class ClassJobFormatter : IValueFormatter
{
    public bool CanFormat(object value) => value is ClassJob;

    public string Format(object value, string subKey, string format, int? precision)
    {
        var job = (ClassJob)value;
        return precision == 3 ? job.Abbreviation.ToString() : job.NameEnglish.ToString();
    }
}
