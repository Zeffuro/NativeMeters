using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using NativeMeters.Services;

namespace NativeMeters.Config;

public abstract class BaseConfig<T> where T : BaseConfig<T>, new()
{
    protected abstract string FileName { get; }

    public static T Instance { get; } = Load();

    [JsonIgnore] public Action? OnSave { get; set; }

    private static T Load()
    {
        try
        {
            if (File.Exists(new T().FileName))
            {
                var fileText = File.ReadAllText(new T().FileName);
                var config = JsonSerializer.Deserialize<T>(fileText);
                if (config != null)
                    return config;
            }
        }
        catch (Exception e)
        {
            Service.Logger.Error(e, $"Error loading config {new T().FileName}, creating new.");
        }
        var newConfig = new T();
        newConfig.Save();
        return newConfig;
    }

    public void Save()
    {
        try
        {
            var fileText = JsonSerializer.Serialize(this);
            File.WriteAllText(FileName, fileText);
            OnSave?.Invoke();
        }
        catch (Exception e)
        {
            Service.Logger.Error(e, $"Error saving config {FileName}");
        }
        OnSave?.Invoke();
    }
}