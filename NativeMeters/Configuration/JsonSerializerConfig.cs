using System.Text.Json;
using System.Text.Json.Serialization;

namespace NativeMeters.Configuration;

public static class JsonSerializerConfig
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static readonly JsonSerializerOptions CaseSensitive = new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNameCaseInsensitive = false,
    };

    public static readonly JsonSerializerOptions Compact = new()
    {
        WriteIndented = false,
        IncludeFields = true,
    };
}
