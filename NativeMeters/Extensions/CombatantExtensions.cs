using System;
using System.Numerics;
using NativeMeters.Models;

namespace NativeMeters.Extensions;

public static class CombatantExtensions
{
    private const uint LimitBreakIconId = 103;

    extension(Combatant combatant)
    {
        public uint GetIconId(JobIconType iconType = JobIconType.Default)
        {
            // Check for Limit Break specifically
            if (string.Equals(combatant.Name, "Limit Break", StringComparison.OrdinalIgnoreCase))
            {
                return LimitBreakIconId;
            }

            if (combatant.Job.RowId == 0)
            {
                return 0;
            }

            return JobIconMaps.GetIcon(combatant.Job.RowId, iconType);
        }

        public Vector4 GetColor()
        {
            if (string.Equals(combatant.Name, "Limit Break", StringComparison.OrdinalIgnoreCase))
            {
                return JobColorMaps.DefaultColors[0];
            }

            if (combatant.Job.RowId != 0 && JobColorMaps.DefaultColors.TryGetValue(combatant.Job.RowId, out var color))
            {
                return color;
            }

            return JobColorMaps.DefaultColors[0];
        }
    }
}