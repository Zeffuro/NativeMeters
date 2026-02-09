using System;
using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

public class EncounterState
{
    public DateTime StartTime { get; private set; } = DateTime.MinValue;
    public DateTime LastActionTime { get; private set; } = DateTime.MinValue;
    public DateTime EndTime { get; private set; } = DateTime.MinValue;
    public bool IsActive { get; private set; }

    public string? EncounterName { get; set; }
    public string? ZoneName { get; private set; }

    public void EnsureStarted()
    {
        if (!IsActive) Start();
    }

    public void Start()
    {
        if (IsActive) return;
        StartTime = DateTime.Now;
        LastActionTime = DateTime.Now;
        EndTime = DateTime.MinValue;
        IsActive = true;

        ZoneName = GetCurrentZoneName();
        EncounterName = ZoneName;
    }

    public void UpdateLastAction() => LastActionTime = DateTime.Now;

    public void End()
    {
        if (!IsActive) return;
        EndTime = DateTime.Now;
        IsActive = false;
    }

    public TimeSpan GetDuration()
    {
        if (StartTime == DateTime.MinValue) return TimeSpan.Zero;

        DateTime end;
        if (IsActive)
        {
            end = DateTime.Now;
        }
        else
        {
            end = LastActionTime != DateTime.MinValue ? LastActionTime : EndTime;
        }

        var duration = end - StartTime;
        return duration.TotalSeconds < 1 ? TimeSpan.FromSeconds(1) : duration;
    }

    private static string GetCurrentZoneName()
    {
        var territoryId = Service.ClientState.TerritoryType;
        if (territoryId == 0) return "Unknown";
        var territory = Service.DataManager.GetExcelSheet<TerritoryType>().GetRowOrDefault(territoryId);
        var placeName = territory?.PlaceName.ValueNullable?.Name.ToString();
        return !string.IsNullOrEmpty(placeName) ? placeName : "Unknown";
    }
}
