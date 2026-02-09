using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace NativeMeters.Nodes.Configuration.Meter;

public static class IconRegistry
{
    private const int MaxIconId = 250_000;
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1:D6}.tex";

    public static List<uint> ValidIconIds { get; private set; } = [];
    public static bool IsLoading { get; private set; } = false;
    public static bool IsLoaded { get; private set; } = false;

    public static void Initialize(IDataManager dataManager)
    {
        if (IsLoaded || IsLoading) return;

        IsLoading = true;
        Task.Run(() =>
        {
            var results = new List<uint>(20000);
            for (uint i = 0; i < MaxIconId; i++)
            {
                var path = string.Format(IconFileFormat, i / 1000, i);

                if (dataManager.FileExists(path))
                {
                    results.Add(i);
                }
            }

            ValidIconIds = results;
            IsLoaded = true;
            IsLoading = false;
        });
    }
}
