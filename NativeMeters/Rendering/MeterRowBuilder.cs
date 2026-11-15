using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Tags;

namespace NativeMeters.Rendering;

public sealed record CombatantRowData(Combatant Combatant, MeterSettings Settings, int Rank, int CombatantCount)
    : CombatantTagContext(Combatant, Rank, CombatantCount);

internal sealed record MeterRows(List<CombatantRowData> Expanded, List<CombatantRowData> Self);

internal static class MeterRowBuilder
{
    public static MeterRows Build(List<Combatant> combatants, MeterSettings settings, bool preview)
    {
        combatants.RemoveAll(c => c.IsLimitBreak ? !settings.ShowLimitBreak
            : !settings.ShowNonPlayerCombatants && c.Job.RowId == 0);

        var self = combatants.Find(c => c.IsSelf);
        if (self == null && preview) self = combatants.FirstOrDefault();

        var selector = StatSelector.GetStatSelector(settings.StatToTrack);
        combatants.Sort((left, right) => selector(right).CompareTo(selector(left)));
        var rows = new List<CombatantRowData>(combatants.Count);
        List<CombatantRowData> selfRows = [];
        for (var i = 0; i < combatants.Count; i++)
        {
            var row = new CombatantRowData(combatants[i], settings, i + 1, combatants.Count);
            rows.Add(row);
            if (ReferenceEquals(row.Combatant, self)) selfRows.Add(row);
        }

        if (settings.PinSelfToTop && selfRows.Count != 0)
        {
            rows.Remove(selfRows[0]);
            rows.Insert(0, selfRows[0]);
        }
        var maxRows = Math.Max(0, settings.MaxCombatants);
        if (rows.Count > maxRows)
            rows.RemoveRange(maxRows, rows.Count - maxRows);

        return new MeterRows(rows, selfRows);
    }

    public static Vector2 GetDisplaySize(MeterSettings settings, bool hasSelf)
    {
        if (!settings.IsCollapsed || !settings.KeepSelfVisibleWhenCollapsed) return settings.Size;
        var inset = settings.ShowWindowBackground
            && settings.BackgroundStyle is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused ? 8f : 0f;
        var header = settings.HeaderEnabled ? settings.HeaderHeight : 0f;
        var rowPitch = hasSelf ? MathF.Ceiling(settings.RowHeight + settings.RowSpacing) : 0f;
        return new Vector2(settings.Size.X, MathF.Ceiling(header + rowPitch + inset * 2));
    }

    public static int GetVisibleRowCount(float height, float rowHeight, float spacing)
        => (int)(height / Math.Max(1f, rowHeight + spacing));
}
