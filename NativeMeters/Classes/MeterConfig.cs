using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using NativeMeters.Services;

namespace NativeMeters.Classes;

public abstract class MeterConfig<T> : ISavable where T : MeterConfig<T>, new() {
    protected abstract string FileName { get; }

    public static T Load() {
        var configFileName = new T().FileName;

        Service.Logger.Debug($"Loading Config {configFileName}");
        try {
            if (File.Exists(configFileName)) {
                var fileText = File.ReadAllText(configFileName);
                var config = JsonSerializer.Deserialize<T>(fileText);
                if (config != null)
                    return config;
            }
        } catch (Exception e) {
            Service.Logger.Error(e, $"Error loading config {configFileName}, creating new.");
        }

        var newConfig = new T();
        newConfig.Save();
        return newConfig;
    }

    public void Save() {
        Service.Logger.Debug($"Saving Config {FileName}");
        try {
            var fileText = JsonSerializer.Serialize(this);
            File.WriteAllText(FileName, fileText);
            OnSave?.Invoke();
        } catch (Exception e) {
            Service.Logger.Error(e, $"Error saving config {FileName}");
        }
        OnSave?.Invoke();
    }

    [JsonIgnore] public Action? OnSave { get; set; }
}