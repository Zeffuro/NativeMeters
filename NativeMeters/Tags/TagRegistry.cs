using System;
using System.Collections.Generic;
using System.Linq;
using NativeMeters.Models;

namespace NativeMeters.Tags;

public record TagInfo(string Category, string Name, string Tag, string Description, string Example);

public static class TagRegistry
{
    public static List<TagInfo> GetAllTags()
    {
        var list = new List<TagInfo>
        {
            new("1. Common", "Name", "[name]", "Player Name (Row) or Encounter Name (Header)", "John Doe / Boss Name"),
            new("1. Common", "First Name", "[name_first]", "First name only", "John"),
            new("1. Common", "Last Name", "[name_last]", "First name only", "John"),
            new("1. Common", "Initials", "[name_first.1].[name_last.1]", "Initials", "J.D"),
            new("1. Common", "Job Abbreviation", "[job]", "Job name", "Warrior"),
            new("1. Common", "Job Abbreviation", "[job.3]", "3-letter job abbreviation", "WAR"),
            new("1. Common", "Duration", "[duration]", "Combat time (mm:ss)", "04:20"),
            new("1. Common", "Zone Name", "[zone]", "Current zone/instance", "The Goblet"),

            new("2. Damage", "Encounter DPS", "[encdps.0]", "Damage per second across entire encounter", "12345"),
            new("2. Damage", "Encounter DPS (Kilo)", "[encdps:k.1]", "Damage per second in thousands", "12.3k"),
            new("2. Damage", "Active DPS", "[dps.0]", "Damage per second while actively attacking", "12500"),
            new("2. Damage", "Damage Total", "[damage:m.2]", "Total damage dealt in millions", "4.20m"),
            new("2. Damage", "Damage %", "[damagepct.0]%", "Player's % of Raid Damage (Row only)", "24%"),
            new("2. Damage", "Crit %", "[crithitpct.0]%", "Critical hit percentage", "22%"),
            new("2. Damage", "Direct Hit %", "[directhitpct.0]%", "Direct hit percentage", "35%"),
            new("2. Damage", "Crit Direct Hit %", "[critdirecthitpct.0]%", "Critical Direct Hit percentage", "10%"),
            new("2. Damage", "Max Hit: Full", "[maxhit]", "Full max hit string (Skill-Value)", "Midare Setsugekka-55200"),
            new("2. Damage", "Max Hit: Skill", "[maxhit_skill]", "Name of highest damage skill", "Midare Setsugekka"),
            new("2. Damage", "Max Hit: Value", "[maxhit_val:k.1]", "Value of highest damage skill", "55.2k"),
            new("2. Damage", "Last 10s DPS", "[Last10DPS.0]", "DPS over the last 10 seconds", "15000"),
            new("2. Damage", "Last 30s DPS", "[Last30DPS.0]", "DPS over the last 30 seconds", "14000"),
            new("2. Damage", "Last 60s DPS", "[Last60DPS.0]", "DPS over the last 60 seconds", "13500"),
            new("2. Damage", "Swings", "[swings]", "Total number of attack swings", "150"),
            new("2. Damage", "Hits", "[hits]", "Total number of successful hits", "149"),
            new("2. Damage", "Misses", "[misses]", "Total number of missed attacks", "1"),

            new("3. Healing", "Encounter HPS", "[enchps.0]", "Healing per second across entire encounter", "8500"),
            new("3. Healing", "Encounter HPS (Kilo)", "[enchps:k.1]", "Healing per second in thousands", "8.5k"),
            new("3. Healing", "Healed Total", "[healed:k.1]", "Total healing output", "500.5k"),
            new("3. Healing", "Healed %", "[healedpct.0]%", "Player's % of Raid Healing (Row only)", "45%"),
            new("3. Healing", "Overheal Total", "[overHeal:k.1]", "Total healing that exceeded max HP", "150.0k"),
            new("3. Healing", "Overheal %", "[OverHealPct.0]%", "Percentage of healing wasted", "15%"),
            new("3. Healing", "Crit Heal %", "[crithealpct.0]%", "Critical heal percentage", "20%"),
            new("3. Healing", "Damage Shield", "[damageShield:k.1]", "Total shields applied", "50.0k"),
            new("3. Healing", "Absorb Heal", "[absorbHeal:k.1]", "Total HP absorbed", "10.0k"),
            new("3. Healing", "Max Heal: Full", "[maxheal]", "Full max heal string", "Cure III-35000"),
            new("3. Healing", "Max Heal: Skill", "[maxheal_skill]", "Name of highest healing skill", "Cure III"),
            new("3. Healing", "Max Heal: Value", "[maxheal_val:k.1]", "Value of highest healing skill", "35.0k"),

            new("4. Defense", "Damage Taken", "[damagetaken:k.1]", "Total damage received", "45.2k"),
            new("4. Defense", "Heals Taken", "[healstaken:k.1]", "Total heals received", "50.5k"),
            new("4. Defense", "Parry %", "[ParryPct.0]%", "Percentage of attacks parried", "15%"),
            new("4. Defense", "Block %", "[BlockPct.0]%", "Percentage of attacks blocked", "10%"),
            new("4. Defense", "Deaths", "[deaths]", "Number of times died", "2"),

            new("5. Custom", "Target HP %", "[targethppct.1]%", "Current HP% of your target", "45.2%"),
            new("5. Custom", "Focus Target HP %", "[focushppct.1]%", "Focus HP% of your target", "36.8%"),
            new("5. Custom", "Target Name", "[targetname]", "Name of your current target", "Striking Dummy"),
            new("5. Custom", "Focus Target Name", "[focusname]", "Name of your focus target", "Striking Dummy"),
        };

        var explicitlyAdded = new HashSet<string>(list.Select(t => t.Tag.Replace("[", "").Replace("]", "").Replace("%", "").Split('.')[0].Split(':')[0]), StringComparer.OrdinalIgnoreCase);

        foreach (var prop in typeof(Combatant).GetProperties())
        {
            if (explicitlyAdded.Contains(prop.Name)) continue;

            list.Add(new TagInfo("6. Raw Data (Combatant)", prop.Name, $"[{prop.Name}]", $"Raw property: {prop.Name}", ""));
        }

        foreach (var prop in typeof(Encounter).GetProperties())
        {
            if (explicitlyAdded.Contains(prop.Name)) continue;

            list.Add(new TagInfo("7. Raw Data (Encounter)", prop.Name, $"[{prop.Name}]", $"Raw property: {prop.Name}", ""));
        }

        return list;
    }
}
