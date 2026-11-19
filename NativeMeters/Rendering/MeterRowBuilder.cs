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
    : CombatantTagContext(Combatant, Rank, CombatantCount)
{
    internal Func<Func<Combatant, double>, double>? SampleMaxStat { get; init; }
}

internal sealed record MeterRows(List<CombatantRowData> Expanded, List<CombatantRowData> Self);

internal static class MeterRowBuilder
{
    public static MeterRows Build(List<Combatant> combatants, MeterSettings settings, bool preview,
        Func<Func<Combatant, double>, double>? sampleMaxStat = null)
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
            var row = new CombatantRowData(combatants[i], settings, i + 1, combatants.Count) { SampleMaxStat = sampleMaxStat };
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
        if (!settings.IsCollapsed ||
            (!settings.KeepSelfVisibleWhenCollapsed && settings.Layout != MeterLayout.Horizontal))
            return settings.Size;

        var inset = settings.ShowWindowBackground
            && settings.BackgroundStyle is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused ? 8f : 0f;
        var header = settings.HeaderEnabled ? settings.HeaderHeight : 0f;
        var rowPitch = hasSelf && settings.KeepSelfVisibleWhenCollapsed
            ? MathF.Ceiling(settings.RowHeight + (settings.Layout == MeterLayout.Horizontal ? 0f : settings.RowSpacing))
            : 0f;
        return new Vector2(settings.Size.X, MathF.Ceiling(header + rowPitch + inset * 2));
    }

    public static float GetHorizontalCardSpacing(float contentWidth, MeterSettings settings)
    {
        var slots = Math.Max(1, settings.MaxCombatants);
        if (slots == 1) return 0;

        var minimumCardWidth = Math.Min(16, MathF.Floor(contentWidth / slots));
        var maxGap = MathF.Floor(Math.Max(0, (contentWidth - minimumCardWidth * slots) / (slots - 1)));
        return Math.Clamp(settings.RowSpacing, 0, maxGap);
    }

    public static float GetHorizontalCardWidth(float contentWidth, MeterSettings settings)
    {
        var slots = Math.Max(1, settings.MaxCombatants);
        var gap = GetHorizontalCardSpacing(contentWidth, settings);
        return MathF.Floor(Math.Max(0, contentWidth - gap * (slots - 1)) / slots);
    }

    public static bool TryFitCardComponent(Vector2 cardSize, ComponentSettings settings, out Vector2 size, out Vector2 position)
    {
        position = Vector2.Max(settings.Position, Vector2.Zero);
        var available = cardSize - position;
        size = new Vector2(
            MathF.Floor(settings.Size.X > 0 ? Math.Min(settings.Size.X, available.X) : available.X),
            MathF.Floor(settings.Size.Y > 0 ? Math.Min(settings.Size.Y, available.Y) : available.Y));
        return size is { X: >= 1, Y: >= 1 };
    }

    public static int GetVisibleRowCount(float height, float rowHeight, float spacing)
        => (int)(height / Math.Max(1f, rowHeight + spacing));
}
