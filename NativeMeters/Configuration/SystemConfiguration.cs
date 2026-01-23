using System.Collections.Generic;
using NativeMeters.Helpers;

namespace NativeMeters.Configuration;

public class SystemConfiguration
{
    public const string FileName = "NativeMeters.json";
    public GeneralSettings General { get; set; } = new();
    public ConnectionSettings ConnectionSettings { get; set; } = new();

    public List<MeterSettings> Meters { get; set; } = new();

    public void EnsureInitialized()
    {
        if (Meters.Count == 0)
        {
            var defaultMeter = new MeterSettings { Name = "DPS" };
            MeterPresets.ApplyDefaultStylish(defaultMeter);
            Meters.Add(defaultMeter);
        }
    }
}
