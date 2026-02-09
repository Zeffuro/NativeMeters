namespace NativeMeters.Models.Internal;

public readonly struct ActionResultEvent
{
    public ulong SourceId { get; init; }
    public string SourceName { get; init; }
    public uint SourceJobId { get; init; }

    public uint TargetId { get; init; }
    public string TargetName { get; init; }
    public uint TargetJobId { get; init; }
    public uint TargetCurrentHp { get; init; }
    public uint TargetMaxHp { get; init; }
    public bool IsPlayerTarget { get; init; }

    public long Damage { get; init; }
    public long Healing { get; init; }
    public long OverHeal { get; init; }

    public bool IsCrit { get; init; }
    public bool IsDirectHit { get; init; }
    public bool IsMiss { get; init; }

    public uint ActionId { get; init; }
    public bool IsDamageTakenOnly { get; init; }
}
