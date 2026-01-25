using System;
using System.IO;
using System.Text.Json;
using Dalamud.Utility;
using NativeMeters.Configuration;
using NativeMeters.Services;

namespace NativeMeters.Utilities;

public static class JsonFileWrapper {
    private static readonly JsonSerializerOptions SerializerOptions = new() {
        WriteIndented = true,
        IncludeFields = true,
    };

    public static T LoadFile<T>(string filePath) where T : new()
    {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo is { Exists: true })
        {
            try
            {
                var fileText = File.ReadAllText(fileInfo.FullName);
                var dataObject = JsonSerializer.Deserialize<T>(fileText, JsonSerializerConfig.Default);

                if (dataObject is null)
                {
                    dataObject = new T();
                    SaveFile(dataObject, filePath);
                }

                return dataObject;
            }
            catch (Exception e)
            {
                Service.Logger.Error(e, $"Error trying to load file {filePath}, creating a new one instead.");
                SaveFile(new T(), filePath);
            }
        }

        var newFile = new T();
        SaveFile(newFile, filePath);
        return newFile;
    }

    public static void SaveFile<T>(T? file, string filePath)
    {
        try
        {
            if (file is null)
            {
                Service.Logger.Error("Null file provided.");
                return;
            }

            var fileText = JsonSerializer.Serialize(file, file.GetType(), JsonSerializerConfig.Default);
            FilesystemUtil.WriteAllTextSafe(filePath, fileText);
        }
        catch (Exception e)
        {
            Service.Logger.Error(e, $"Error trying to save file {filePath}");
        }
    }

    public static FileInfo GetFileInfo(params string[] path) {
        var directory = Service.PluginInterface.ConfigDirectory;

        for (var index = 0; index < path.Length - 1; index++) {
            directory = new DirectoryInfo(Path.Combine(directory.FullName, path[index]));
            if (!directory.Exists) {
                directory.Create();
            }
        }

        return new FileInfo(Path.Combine(directory.FullName, path[^1]));
    }
}
