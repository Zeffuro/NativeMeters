using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            "TimeSpan" => TimeSpan.TryParse(stringValue, out TimeSpan tsResult) ? tsResult : TimeSpan.Zero,
            _ => default(T)
        };

        return (T)result;
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
}