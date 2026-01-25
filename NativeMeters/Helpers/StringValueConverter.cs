using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NativeMeters.Extensions;
using NativeMeters.Services;

namespace NativeMeters.Helpers;

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
            "Int32" => TryParseWithSuffix(stringValue, out double intVal) ? (int)intVal : 0,
            "Int64" => TryParseWithSuffix(stringValue, out double longVal) ? (long)longVal : 0L,
            "Double" => TryParseWithSuffix(stringValue, out double doubleVal) ? doubleVal : 0.0,
            "Boolean" => bool.TryParse(stringValue, out bool boolResult) && boolResult,
            "TimeSpan" => ParseTimeSpan(stringValue),
            "ClassJob" => Service.DataManager.GetClassJobByAbbreviation(stringValue),
            _ => default(T)
        };

        return (T)result!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // For serialization, convert the value back to a string.
        writer.WriteStringValue(value?.ToString());
    }

    private static bool TryParseWithSuffix(string input, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim().ToUpperInvariant();

        char suffix = input.Length > 0 ? input[^1] : '\0';
        (double multiplier, input) = suffix switch
        {
            'K' => (1_000, input[..^1]),
            'M' => (1_000_000, input[..^1]),
            'B' => (1_000_000_000, input[..^1]),
            _   => (1, input)
        };

        if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            value = parsed * multiplier;
            return true;
        }

        return false;
    }

    private static TimeSpan ParseTimeSpan(string? s)
    {
        if (string.IsNullOrEmpty(s)) return TimeSpan.Zero;

        if (s.Contains(':'))
        {
            var parts = s.Split(':');
            if (parts.Length == 2) // MM:SS
            {
                if (int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var s1))
                    return new TimeSpan(0, 0, m, s1);
            }
            if (int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m1) && int.TryParse(parts[2], out var s2))
                return new TimeSpan(0, h, m1, s2);
        }

        if (double.TryParse(s, CultureInfo.InvariantCulture, out var seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        return TimeSpan.Zero;
    }
}
