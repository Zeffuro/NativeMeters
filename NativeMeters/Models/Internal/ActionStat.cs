namespace NativeMeters.Models.Internal;

public class ActionStat
{
    public uint ActionId { get; init; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int Hits { get; set; }
    public int CritHits { get; set; }
    public int DirectHits { get; set; }
    public long MaxHit { get; set; }
}
