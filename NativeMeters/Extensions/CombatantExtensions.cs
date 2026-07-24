using System;
using System.Numerics;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Rendering;
using NativeMeters.Services;

namespace NativeMeters.Extensions;

public static class CombatantExtensions
{
    private const uint LimitBreakIconId = 103;

    extension(Combatant combatant)
    {
        public uint GetIconId(JobIconType iconType = JobIconType.Default)
        {
            if (string.Equals(combatant.Name, "Limit Break", StringComparison.OrdinalIgnoreCase))
                return LimitBreakIconId;

            if (combatant.Job.RowId == 0)
                return 0;

            return JobIconMaps.GetIcon(combatant.Job.RowId, iconType);
        }

        public Vector4 GetColor() => ColorResolver.GetDefaultColor(combatant);

        public Vector4 GetColor(ColorMode mode, ComponentSettings? settings = null, MeterSettings? meterSettings = null)
            => ColorResolver.GetColor(combatant, mode, settings, meterSettings);

        public bool IsLimitBreak => combatant.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase);

        public bool IsYou => combatant.Name.Equals("YOU", StringComparison.OrdinalIgnoreCase);

        public bool IsSelf
        {
            get
            {
                if (combatant.IsYou) return true;

                var localPlayerName = Service.ObjectTable.LocalPlayer?.Name.TextValue;
                return !string.IsNullOrWhiteSpace(localPlayerName)
                       && combatant.Name.Equals(localPlayerName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
