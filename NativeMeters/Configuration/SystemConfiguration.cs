using System.Collections.Generic;

namespace NativeMeters.Configuration;

public class SystemConfiguration
{
    public const string FileName = "NativeMeters.json";
    public GeneralSettings General { get; set; } = new();
    public ConnectionSettings ConnectionSettings { get; set; } = new();

    // The collection of meters
    public List<MeterSettings> Meters { get; set; } = new();

    public void EnsureInitialized()
    {
        if (Meters.Count == 0)
        {
            Meters.Add(new MeterSettings { Name = "DPS" });
        }
    }
}