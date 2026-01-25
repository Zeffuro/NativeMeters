using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NativeMeters.Configuration;

namespace NativeMeters.Configuration.Persistence;

public static class ConfigSerializer
{
    public static string SerializeConfig(SystemConfiguration config)
        => SerializeCompressed(config);

    public static SystemConfiguration? DeserializeConfig(string input)
        => DeserializeCompressed<SystemConfiguration>(input);

    public static string SerializeCompressed<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonSerializerConfig.Default);
        return CompressToBase64(json);
    }

    public static T? DeserializeCompressed<T>(string input)
    {
        try
        {
            var json = DecompressFromBase64(input);
            return JsonSerializer.Deserialize<T>(json, JsonSerializerConfig.Default);
        }
        catch
        {
            return default;
        }
    }

    public static string SerializeHashSet(HashSet<uint> hashSet)
        => CompressToBase64(string.Join(",", hashSet.OrderBy(x => x)));

    public static HashSet<uint> DeserializeHashSet(string input)
    {
        try
        {
            var data = DecompressFromBase64(input);
            return data
                .Split([','], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => uint.TryParse(s, out var val) ? val : (uint?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToHashSet();
        }
        catch
        {
            return new HashSet<uint>();
        }
    }

    private static string CompressToBase64(string str)
        => Convert.ToBase64String(Dalamud.Utility.Util.CompressString(str));

    private static string DecompressFromBase64(string base64)
        => Dalamud.Utility.Util.DecompressString(Convert.FromBase64String(base64));
}
