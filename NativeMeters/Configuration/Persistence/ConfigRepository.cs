using System;
using System.Threading;
using System.Threading.Tasks;
using NativeMeters.Utilities;

namespace NativeMeters.Configuration.Persistence;

public static class ConfigRepository
{
    private static CancellationTokenSource? _saveTokenSource;

    public static SystemConfiguration LoadOrDefault()
    {
        var file = JsonFileWrapper.GetFileInfo(SystemConfiguration.FileName);
        var config = file.Exists
            ? JsonFileWrapper.LoadFile<SystemConfiguration>(file.FullName)
            : new SystemConfiguration { SetupStatus = SetupStatus.Pending };
        config.EnsureInitialized();
        return config;
    }

    public static SystemConfiguration Reset()
    {
        return new SystemConfiguration();
    }

    public static void Save(SystemConfiguration config)
    {
        _saveTokenSource?.Cancel();
        _saveTokenSource = new CancellationTokenSource();
        var token = _saveTokenSource.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(100, token);
                if (token.IsCancellationRequested) return;

                var file = JsonFileWrapper.GetFileInfo(SystemConfiguration.FileName);
                JsonFileWrapper.SaveFile(config, file.FullName);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
        }, token);
    }

    public static void SaveImmediately(SystemConfiguration config)
    {
        Clear();

        var file = JsonFileWrapper.GetFileInfo(SystemConfiguration.FileName);
        JsonFileWrapper.SaveFile(config, file.FullName);
    }

    public static void Clear()
    {
        _saveTokenSource?.Cancel();
        _saveTokenSource?.Dispose();
        _saveTokenSource = null;
    }
}
