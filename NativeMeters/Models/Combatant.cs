using System;
using System.Text.Json.Serialization;
using Lumina.Excel.Sheets;
using NativeMeters.Data.Serialization;

namespace NativeMeters.Models;

public class Combatant : IEquatable<Combatant>
{
    // String properties that do not need conversion
    [JsonPropertyName("n")]
    public string N { get; set; } = null!;
    [JsonPropertyName("t")]
    public string T { get; set; } = null!;
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    // Double properties for values with decimal places or large numbers with units
    [JsonPropertyName("duration")]
    [JsonConverter(typeof(StringValueConverter<TimeSpan>))]
    public TimeSpan Duration { get; set; }
    [JsonPropertyName("DURATION")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DURATION { get; set; }
    [JsonPropertyName("damage-m")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamageM { get; set; }
    [JsonPropertyName("damage-b")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamageB { get; set; }
    [JsonPropertyName("damage-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamageStar { get; set; }
    [JsonPropertyName("DAMAGE-k")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DAMAGEK { get; set; }
    [JsonPropertyName("DAMAGE-m")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DAMAGEM { get; set; }
    [JsonPropertyName("DAMAGE-b")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DAMAGEB2 { get; set; }
    [JsonPropertyName("DAMAGE-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DAMAGEStar { get; set; }
    [JsonPropertyName("damage%")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamagePercent { get; set; }
    [JsonPropertyName("dps")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Dps { get; set; }
    [JsonPropertyName("dps-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DpsStar { get; set; }
    [JsonPropertyName("DPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DPS { get; set; }
    [JsonPropertyName("DPS-k")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DPSK { get; set; }
    [JsonPropertyName("DPS-m")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DPSM { get; set; }
    [JsonPropertyName("DPS-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DPSStar { get; set; }
    [JsonPropertyName("encdps")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Encdps { get; set; }
    [JsonPropertyName("encdps-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double EncdpsStar { get; set; }
    [JsonPropertyName("ENCDPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCDPS { get; set; }
    [JsonPropertyName("ENCDPS-k")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCDPSK { get; set; }
    [JsonPropertyName("ENCDPS-m")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCDPSM { get; set; }
    [JsonPropertyName("ENCDPS-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCDPSStar { get; set; }
    [JsonPropertyName("crithit%")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double CrithitPercent { get; set; }
    [JsonPropertyName("tohit")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Tohit { get; set; }
    [JsonPropertyName("TOHIT")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double TOHIT { get; set; }
    [JsonPropertyName("maxhit")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Maxhit { get; set; }
    [JsonPropertyName("MAXHIT")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MAXHIT { get; set; }
    [JsonPropertyName("maxhit-*")]
    public string MaxhitStar { get; set; } = null!;
    [JsonPropertyName("MAXHIT-*")]
    public string MAXHITStar { get; set; } = null!;
    [JsonPropertyName("healed%")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double HealedPercent { get; set; }
    [JsonPropertyName("enchps")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Enchps { get; set; }
    [JsonPropertyName("enchps-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double EnchpsStar { get; set; }
    [JsonPropertyName("ENCHPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCHPS { get; set; }
    [JsonPropertyName("ENCHPS-k")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCHPSK { get; set; }
    [JsonPropertyName("ENCHPS-m")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCHPSM { get; set; }
    [JsonPropertyName("ENCHPS-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ENCHPSStar { get; set; }
    [JsonPropertyName("critheal%")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double CrithealPercent { get; set; }
    [JsonPropertyName("maxheal")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Maxheal { get; set; }
    [JsonPropertyName("MAXHEAL")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MAXHEAL { get; set; }
    [JsonPropertyName("maxhealward")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Maxhealward { get; set; }
    [JsonPropertyName("MAXHEALWARD")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MAXHEALWARD { get; set; }
    [JsonPropertyName("maxheal-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MaxhealStar { get; set; }
    [JsonPropertyName("MAXHEAL-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MAXHEALStar { get; set; }
    [JsonPropertyName("maxhealward-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MaxhealwardStar { get; set; }
    [JsonPropertyName("MAXHEALWARD-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double MAXHEALWARDStar { get; set; }
    [JsonPropertyName("damagetaken")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Damagetaken { get; set; }
    [JsonPropertyName("damagetaken-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamagetakenStar { get; set; }
    [JsonPropertyName("healstaken")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Healstaken { get; set; }
    [JsonPropertyName("healstaken-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double HealstakenStar { get; set; }
    [JsonPropertyName("powerdrain")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Powerdrain { get; set; }
    [JsonPropertyName("powerdrain-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double PowerdrainStar { get; set; }
    [JsonPropertyName("powerheal")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Powerheal { get; set; }
    [JsonPropertyName("powerheal-*")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double PowerhealStar { get; set; }
    [JsonPropertyName("Last10DPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Last10DPS { get; set; }
    [JsonPropertyName("Last30DPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Last30DPS { get; set; }
    [JsonPropertyName("Last60DPS")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double Last60DPS { get; set; }
    [JsonPropertyName("ParryPct")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double ParryPct { get; set; }
    [JsonPropertyName("BlockPct")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double BlockPct { get; set; }
    [JsonPropertyName("IncToHit")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double IncToHit { get; set; }
    [JsonPropertyName("OverHealPct")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double OverHealPct { get; set; }
    [JsonPropertyName("DirectHitPct")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DirectHitPct { get; set; }
    [JsonPropertyName("CritDirectHitPct")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double CritDirectHitPct { get; set; }
    [JsonPropertyName("overHeal")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double OverHeal { get; set; }
    [JsonPropertyName("damageShield")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double DamageShield { get; set; }
    [JsonPropertyName("absorbHeal")]
    [JsonConverter(typeof(StringValueConverter<double>))]
    public double AbsorbHeal { get; set; }


    // Long property for large damage values
    [JsonPropertyName("damage")]
    [JsonConverter(typeof(StringValueConverter<long>))]
    public long Damage { get; set; }


    // Integer properties for counts
    [JsonPropertyName("hits")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Hits { get; set; }
    [JsonPropertyName("crithits")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Crithits { get; set; }
    [JsonPropertyName("misses")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Misses { get; set; }
    [JsonPropertyName("hitfailed")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Hitfailed { get; set; }
    [JsonPropertyName("swings")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Swings { get; set; }
    [JsonPropertyName("healed")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Healed { get; set; }
    [JsonPropertyName("critheals")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Critheals { get; set; }
    [JsonPropertyName("heals")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Heals { get; set; }
    [JsonPropertyName("cures")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Cures { get; set; }
    [JsonPropertyName("kills")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Kills { get; set; }
    [JsonPropertyName("deaths")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int Deaths { get; set; }
    [JsonPropertyName("DirectHitCount")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int DirectHitCount { get; set; }
    [JsonPropertyName("CritDirectHitCount")]
    [JsonConverter(typeof(StringValueConverter<int>))]
    public int CritDirectHitCount { get; set; }


    // Boolean property
    [JsonPropertyName("isActive")]
    [JsonConverter(typeof(StringValueConverter<bool>))]
    public bool IsActive { get; set; }


    [JsonPropertyName("crittypes")]
    public string CritTypes { get; set; } = null!;
    [JsonPropertyName("threatstr")]
    public string ThreatStr { get; set; } = null!;
    [JsonPropertyName("threatdelta")]
    public string ThreatDelta { get; set; } = null!;
    [JsonPropertyName("Job")]
    [JsonConverter(typeof(StringValueConverter<ClassJob>))]
    public ClassJob Job { get; set; }

    [JsonIgnore]
    public int? PrivacyIndex { get; set; }

    public bool Equals(Combatant? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Job.RowId == other.Job.RowId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Combatant)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Job);
    }
}
