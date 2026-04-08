using System.IO;
using System.Text.Json;
using NativeMeters.Utilities;

namespace NativeMeters.Configuration;

public static class ConfigMigrator
{
    public static void MigrateIfNeeded(SystemConfiguration config)
    {
        if (config.ConfigVersion < 2)
        {
            config.Visibility.HideWithNativeUi = ReadLegacyValue("General", "HideWithNativeUi", true);
            config.ConfigVersion = 2;
        }
    }

    private static T ReadLegacyValue<T>(T defaultValue, params string[] path)
    {
        try
        {
            var file = JsonFileWrapper.GetFileInfo(SystemConfiguration.FileName);
            if (!file.Exists) return defaultValue;

            using var doc = JsonDocument.Parse(File.ReadAllText(file.FullName));
            var element = doc.RootElement;

            foreach (var key in path)
            {
                if (!element.TryGetProperty(key, out element))
                    return defaultValue;
            }

            return element.Deserialize<T>() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static T ReadLegacyValue<T>(string section, string property, T defaultValue) =>
        ReadLegacyValue(defaultValue, section, property);
}
