using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NativeMeters.Utilities;

namespace NativeMeters.Configuration.Persistence;

public static class ConfigRepository
{
    private static CancellationTokenSource? _saveTokenSource;

    public static SystemConfiguration LoadOrDefault()
    {
        var config = Load() ?? new SystemConfiguration();
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

    private static SystemConfiguration Load()
    {
        var file = JsonFileWrapper.GetFileInfo(SystemConfiguration.FileName);
        var config = JsonFileWrapper.LoadFile<SystemConfiguration>(file.FullName);
        config.EnsureInitialized();
        return config;
    }
}
