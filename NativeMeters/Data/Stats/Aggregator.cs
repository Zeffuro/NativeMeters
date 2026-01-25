using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Utilities;

namespace NativeMeters.Data.Stats;

public static class Aggregator
{
    public static double Sum<T>(IList<T> items, Func<T, double> selector) => items.Sum(selector);
    public static int SumInt<T>(IList<T> items, Func<T, int> selector) => items.Sum(selector);
    public static long SumLong<T>(IList<T> items, Func<T, long> selector) => items.Sum(selector);
    public static double Avg<T>(IList<T> items, Func<T, double> selector) => items.Average(selector);
    public static double Max<T>(IList<T> items, Func<T, double> selector) => items.Max(selector);

    public static TimeSpan AvgTimeSpan<T>(IList<T> items, Func<T, TimeSpan> selector)
        => TimeSpan.FromSeconds(items.Average(c => selector(c).TotalSeconds));

    public static string MaxString<T>(IList<T> items, Func<T, string> selector)
    {
        return items
            .Select(selector)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new { Value = s, Num = ExtractNumericValue(s) })
            .OrderByDescending(x => x.Num)
            .FirstOrDefault()?.Value ?? "";
    }

    private static double ExtractNumericValue(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;

        var parts = s.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            if (NumericParser.TryParse(parts[i], out var num))
                return num;
        }
        return 0;
    }
}
