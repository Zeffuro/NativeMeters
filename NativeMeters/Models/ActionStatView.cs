using System;

namespace NativeMeters.Models;

public class ActionStatView
{
    public uint ActionId { get; init; }
    public string ActionName { get; init; } = "";
    public uint ActionIconId { get; init; }
    public long TotalDamage { get; init; }
    public long TotalHealing { get; init; }
    public int Hits { get; init; }
    public int CritHits { get; init; }
    public int DirectHits { get; init; }
    public long MaxHit { get; init; }
    public double DamagePerSecond { get; init; }
    public double HealingPerSecond { get; init; }

    public DateTime? FirstUsed { get; init; }
    public DateTime? LastUsed { get; init; }
    public TimeSpan ActiveSpan { get; init; }
    public double DamagePercent { get; init; }
    public double HealingPercent { get; init; }
}
