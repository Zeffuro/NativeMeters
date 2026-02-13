using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NativeMeters.Extensions;
using NativeMeters.Services;
using NativeMeters.Utilities;

namespace NativeMeters.Data.Serialization;

public class StringValueConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string but found {reader.TokenType}.");

        var stringValue = reader.GetString();

        if (string.IsNullOrWhiteSpace(stringValue))
            return default;

        object? result = typeToConvert.Name switch
        {
            "Int32" => (int)CleanAndParseNumeric(stringValue),
            "Int64" => (long)CleanAndParseNumeric(stringValue),
            "Double" => CleanAndParseNumeric(stringValue),
            "Boolean" => bool.TryParse(stringValue, out bool boolResult) && boolResult,
            "TimeSpan" => ParseTimeSpan(stringValue),
            "ClassJob" => Service.DataManager.GetClassJobByAbbreviation(stringValue),
            _ => default(T)
        };

        return (T)result!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }

    private static double CleanAndParseNumeric(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;

        if (s == "NaN" || s == "--" || s == "---")
            return 0;

        ReadOnlySpan<char> span = s.AsSpan().Trim();
        if (span.EndsWith("%"))
        {
            span = span.Slice(0, span.Length - 1);
        }

        if (NumericParser.TryParse(span.ToString(), out double result))
        {
            return result;
        }

        return 0;
    }

    private static TimeSpan ParseTimeSpan(string? s)
    {
        if (string.IsNullOrEmpty(s)) return TimeSpan.Zero;

        if (s.Contains(':'))
        {
            var parts = s.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var s1))
                return new TimeSpan(0, 0, m, s1);
            if (parts.Length == 3 && int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m1) && int.TryParse(parts[2], out var s2))
                return new TimeSpan(0, h, m1, s2);
        }

        if (double.TryParse(s, CultureInfo.InvariantCulture, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        return TimeSpan.Zero;
    }
}
