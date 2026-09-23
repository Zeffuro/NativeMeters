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

    public void EnsureStarted(DateTime occurredAt)
    {
        if (!IsActive) Start(occurredAt);
    }

    public void Start(DateTime? occurredAt = null)
    {
        if (IsActive) return;
        StartTime = occurredAt ?? DateTime.UtcNow;
        LastActionTime = occurredAt ?? DateTime.MinValue;
        EndTime = DateTime.MinValue;
        IsActive = true;

        ZoneName = GetCurrentZoneName();
        EncounterName = ZoneName;
    }

    public void UpdateLastAction(DateTime occurredAt)
    {
        if (LastActionTime == DateTime.MinValue || occurredAt < StartTime) StartTime = occurredAt;
        if (occurredAt > LastActionTime) LastActionTime = occurredAt;
    }

    public void End()
    {
        if (!IsActive) return;
        EndTime = DateTime.UtcNow;
        IsActive = false;
    }

    public TimeSpan GetDuration()
    {
        if (StartTime == DateTime.MinValue) return TimeSpan.Zero;

        DateTime end;
        if (IsActive)
        {
            end = DateTime.UtcNow;
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
