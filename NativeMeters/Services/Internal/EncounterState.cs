using System;
using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

public class EncounterState
{
    public DateTime StartTime { get; private set; } = DateTime.MinValue;
    public DateTime EndTime { get; private set; } = DateTime.MinValue;
    public bool IsActive { get; private set; }

    public string? EncounterName { get; set; }
    public string? ZoneName { get; private set; }

    public void Start()
    {
        StartTime = DateTime.Now;
        EndTime = DateTime.MinValue;
        IsActive = true;

        ZoneName = GetCurrentZoneName();
        EncounterName = ZoneName;
    }

    public void End()
    {
        if (!IsActive) return;
        EndTime = DateTime.Now;
        IsActive = false;
    }

    public TimeSpan GetDuration()
    {
        if (StartTime == DateTime.MinValue) return TimeSpan.Zero;
        var end = IsActive ? DateTime.Now : EndTime;
        return end - StartTime;
    }

    private static string GetCurrentZoneName()
    {
        try
        {
            var territoryId = Service.ClientState.TerritoryType;
            if (territoryId == 0) return "Unknown";

            var territory = Service.DataManager.GetExcelSheet<TerritoryType>().GetRowOrDefault(territoryId);
            if (territory == null) return "Unknown";

            var placeName = territory.Value.PlaceName.ValueNullable?.Name.ToString();
            return !string.IsNullOrEmpty(placeName) ? placeName : "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
