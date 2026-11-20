using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Localization;
using NativeMeters.Models;

namespace NativeMeters.Tags;

public record TagInfo(string Category, string Name, string Tag, string Description, string Example);

public static class TagRegistry
{
    public static List<TagInfo> GetAllTags()
    {
        var common = Loc.Get("TagCategoryCommon");
        var damage = Loc.Get("TagCategoryDamage");
        var healing = Loc.Get("TagCategoryHealing");
        var defense = Loc.Get("TagCategoryDefense");
        var custom = Loc.Get("TagCategoryCustom");
        var list = new List<TagInfo>
        {
            new(common, Loc.Get("TagNameFullName"), "[name]", Loc.Get("TagDescFullName"), Loc.Get("TagExampleName")),
            new(common, Loc.Get("TagNameFirstName"), "[name_first]", Loc.Get("TagDescFirstName"), "John"),
            new(common, Loc.Get("TagNameLastName"), "[name_last]", Loc.Get("TagDescLastName"), "Doe"),
            new(common, Loc.Get("TagNameInitials"), "[name_first.1].[name_last.1]", Loc.Get("TagDescInitials"), "J.D"),
            new(common, Loc.Get("TagNameJobName"), "[job]", Loc.Get("TagDescJobName"), Loc.Get("TagExampleWarrior")),
            new(common, Loc.Get("TagNameJobAbbr"), "[job.3]", Loc.Get("TagDescJobAbbr"), "WAR"),
            new(common, Loc.Get("TagNameDuration"), "[duration]", Loc.Get("TagDescDuration"), "04:20"),
            new(common, Loc.Get("TagNameRank"), "[rank]", Loc.Get("TagDescRank"), "2"),
            new(common, Loc.Get("TagNameCount"), "[combatantcount]", Loc.Get("TagDescCount"), "8"),
            new(common, Loc.Get("TagNameRankCount"), "[rank]/[combatantcount]", Loc.Get("TagDescRankCount"), "2/8"),
            new(common, Loc.Get("TagNameZone"), "[zone]", Loc.Get("TagDescZone"), Loc.Get("TagExampleGoblet")),

            new(damage, Loc.Get("TagNameEncounterDps"), "[encdps.0]", Loc.Get("TagDescEncounterDps"), "12345"),
            new(damage, Loc.Get("TagNameEncounterDpsCompact"), "[encdps:c.1]", Loc.Get("TagDescEncounterDpsCompact"), "12.3K"),
            new(damage, Loc.Get("TagNameEncounterDpsKilo"), "[encdps:k.1]", Loc.Get("TagDescEncounterDpsKilo"), "12.3k"),
            new(damage, Loc.Get("TagNameActiveDps"), "[dps.0]", Loc.Get("TagDescActiveDps"), "12500"),
            new(damage, Loc.Get("TagNameDamageTotal"), "[damage:m.2]", Loc.Get("TagDescDamageTotal"), "4.20m"),
            new(damage, Loc.Get("TagNameDamagePct"), "[damagepct.0]%", Loc.Get("TagDescDamagePct"), "24%"),
            new(damage, Loc.Get("TagNameCritPct"), "[crithitpct.0]%", Loc.Get("TagDescCritPct"), "22%"),
            new(damage, Loc.Get("TagNameDirectPct"), "[directhitpct.0]%", Loc.Get("TagDescDirectPct"), "35%"),
            new(damage, Loc.Get("TagNameCritDirectPct"), "[critdirecthitpct.0]%", Loc.Get("TagDescCritDirectPct"), "10%"),
            new(damage, Loc.Get("TagNameMaxHitFull"), "[maxhit]", Loc.Get("TagDescMaxHitFull"), Loc.Get("TagExampleMidare") + "-55200"),
            new(damage, Loc.Get("TagNameMaxHitSkill"), "[maxhit_skill]", Loc.Get("TagDescMaxHitSkill"), Loc.Get("TagExampleMidare")),
            new(damage, Loc.Get("TagNameMaxHitValue"), "[maxhit_val:k.1]", Loc.Get("TagDescMaxHitValue"), "55.2k"),
            new(damage, Loc.Format("TagNameRecentDps", 10), "[Last10DPS.0]", Loc.Format("TagDescRecentDps", 10), "15000"),
            new(damage, Loc.Format("TagNameRecentDps", 30), "[Last30DPS.0]", Loc.Format("TagDescRecentDps", 30), "14000"),
            new(damage, Loc.Format("TagNameRecentDps", 60), "[Last60DPS.0]", Loc.Format("TagDescRecentDps", 60), "13500"),
            new(damage, Loc.Get("TagNameSwings"), "[swings]", Loc.Get("TagDescSwings"), "150"),
            new(damage, Loc.Get("TagNameHits"), "[hits]", Loc.Get("TagDescHits"), "149"),
            new(damage, Loc.Get("TagNameMisses"), "[misses]", Loc.Get("TagDescMisses"), "1"),

            new(healing, Loc.Get("TagNameEncounterHps"), "[enchps.0]", Loc.Get("TagDescEncounterHps"), "8500"),
            new(healing, Loc.Get("TagNameEncounterHpsKilo"), "[enchps:k.1]", Loc.Get("TagDescEncounterHpsKilo"), "8.5k"),
            new(healing, Loc.Get("TagNameHealedTotal"), "[healed:k.1]", Loc.Get("TagDescHealedTotal"), "500.5k"),
            new(healing, Loc.Get("TagNameHealedPct"), "[healedpct.0]%", Loc.Get("TagDescHealedPct"), "45%"),
            new(healing, Loc.Get("TagNameOverheal"), "[overHeal:k.1]", Loc.Get("TagDescOverheal"), "150.0k"),
            new(healing, Loc.Get("TagNameOverhealPct"), "[OverHealPct.0]%", Loc.Get("TagDescOverhealPct"), "15%"),
            new(healing, Loc.Get("TagNameCritHealPct"), "[crithealpct.0]%", Loc.Get("TagDescCritHealPct"), "20%"),
            new(healing, Loc.Get("TagNameShield"), "[damageShield:k.1]", Loc.Get("TagDescShield"), "50.0k"),
            new(healing, Loc.Get("TagNameAbsorb"), "[absorbHeal:k.1]", Loc.Get("TagDescAbsorb"), "10.0k"),
            new(healing, Loc.Get("TagNameMaxHealFull"), "[maxheal]", Loc.Get("TagDescMaxHealFull"), Loc.Get("TagExampleCure") + "-35000"),
            new(healing, Loc.Get("TagNameMaxHealSkill"), "[maxheal_skill]", Loc.Get("TagDescMaxHealSkill"), Loc.Get("TagExampleCure")),
            new(healing, Loc.Get("TagNameMaxHealValue"), "[maxheal_val:k.1]", Loc.Get("TagDescMaxHealValue"), "35.0k"),

            new(defense, Loc.Get("TagNameDamageTaken"), "[damagetaken:k.1]", Loc.Get("TagDescDamageTaken"), "45.2k"),
            new(defense, Loc.Get("TagNameHealsTaken"), "[healstaken:k.1]", Loc.Get("TagDescHealsTaken"), "50.5k"),
            new(defense, Loc.Get("TagNameParryPct"), "[ParryPct.0]%", Loc.Get("TagDescParryPct"), "15%"),
            new(defense, Loc.Get("TagNameBlockPct"), "[BlockPct.0]%", Loc.Get("TagDescBlockPct"), "10%"),
            new(defense, Loc.Get("TagNameDeaths"), "[deaths]", Loc.Get("TagDescDeaths"), "2"),

            new(custom, Loc.Get("TagNameTargetHp"), "[targethppct.1]%", Loc.Get("TagDescTargetHp"), "45.2%"),
            new(custom, Loc.Get("TagNameFocusHp"), "[focushppct.1]%", Loc.Get("TagDescFocusHp"), "36.8%"),
            new(custom, Loc.Get("TagNameTargetName"), "[targetname]", Loc.Get("TagDescTargetName"), Loc.Get("TagExampleDummy")),
            new(custom, Loc.Get("TagNameFocusName"), "[focusname]", Loc.Get("TagDescFocusName"), Loc.Get("TagExampleDummy")),
        };

        var explicitlyAdded = new HashSet<string>(list.Select(t => t.Tag.Replace("[", "").Replace("]", "").Replace("%", "").Split('.')[0].Split(':')[0]), StringComparer.OrdinalIgnoreCase);

        foreach (var prop in typeof(Combatant).GetProperties())
        {
            if (explicitlyAdded.Contains(prop.Name)) continue;

            list.Add(new TagInfo(Loc.Get("TagCategoryCombatant"), prop.Name, $"[{prop.Name}]", Loc.Format("TagRawProperty", prop.Name), ""));
        }

        foreach (var prop in typeof(Encounter).GetProperties())
        {
            if (explicitlyAdded.Contains(prop.Name)) continue;

            list.Add(new TagInfo(Loc.Get("TagCategoryEncounter"), prop.Name, $"[{prop.Name}]", Loc.Format("TagRawProperty", prop.Name), ""));
        }

        return list;
    }
}
