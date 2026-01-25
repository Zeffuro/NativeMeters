using System.Collections.Generic;
using System.Linq;

namespace NativeMeters.Tags;

public static class TagDefinitions
{
    public const string DefaultDropdownLabel = "-- Insert Tag --";

    public static readonly Dictionary<string, string> Templates = new()
    {
        { DefaultDropdownLabel, "" },

        { "Name: Full", "[name]" },
        { "Name: First", "[name_first]" },
        { "Name: Initial", "[name_first.1]" },
        { "Job: Abbr", "[job.3]" },

        { "DPS", "[dps.0]" },
        { "DPS: Raw (No Comma)", "[dps:r.0]" },
        { "DPS: Kilo", "[dps:k.1]" },
        { "Dmg: Total", "[damage.0]" },

        { "MaxHit: Skill Only", "[maxhit_skill]" },
        { "MaxHit: Val Only", "[maxhit_val.0]" },
        { "MaxHit: Full", "[maxhit]" },

        { "Heal: %", "[healedpct.1]%" },
        { "Heal: Overheal %", "[overhealpct.0]%" },
        { "Crit %", "[crithitpct.0]%" },
        { "DH %", "[directhitpct.0]%" },

        { "Enc: Zone", "[zone]" },
        { "Enc: Duration", "[duration]" },
    };

    public static List<string> GetLabels() => Templates.Keys.ToList();
}
