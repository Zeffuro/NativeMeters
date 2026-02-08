using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

public record ActionResultEvent
{
    public ulong SourceId { get; init; }
    public string SourceName { get; init; } = "";
    public ClassJob SourceJob { get; init; }

    public uint TargetId { get; init; }
    public string TargetName { get; init; } = "";
    public ClassJob TargetJob { get; init; }
    public bool IsPlayerTarget { get; init; }

    public long Damage { get; init; }
    public long Healing { get; init; }
    public long OverHeal { get; init; }

    public bool IsCrit { get; init; }
    public bool IsDirectHit { get; init; }

    public uint ActionId { get; init; }
    public bool IsDamageTakenOnly { get; init; }
}
