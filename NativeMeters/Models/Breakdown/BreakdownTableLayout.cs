using System.Collections.Generic;
using System.Linq;
using NativeMeters.Localization;

namespace NativeMeters.Models.Breakdown;

public class BreakdownTableLayout
{
    private List<BreakdownColumn>? cachedVisible;
    private int lastVisibleHash;

    public List<BreakdownColumn> Columns { get; set; } =
    [
        new("Action", Loc.Get("BreakdownAction"), 0, true, ColumnAlignment.Left),
        new("Hits", Loc.Get("BreakdownHits"), 36),
        new("Crit", Loc.Get("BreakdownCritShort"), 40),
        new("DH", Loc.Get("BreakdownDirectShort"), 40),
        new("Pct", "%", 46),
        new("Total", Loc.Get("BreakdownDamage"), 64),
        new("PerSec", "DPS", 48),
        new("Max", Loc.Get("BreakdownMax"), 54),
        new("Active", Loc.Get("BreakdownActive"), 42),
        new("Overheal", Loc.Get("BreakdownOverhealShort"), 45),
    ];

    public IReadOnlyList<BreakdownColumn> VisibleColumns
    {
        get
        {
            int hash = Columns.Count(c => c.IsVisible);
            if (cachedVisible == null || hash != lastVisibleHash)
            {
                cachedVisible = Columns.Where(c => c.IsVisible).ToList();
                lastVisibleHash = hash;
            }
            return cachedVisible;
        }
    }

    public void InvalidateCache() => cachedVisible = null;

    public List<(BreakdownColumn Column, float X, float W)> Resolve(float totalWidth)
    {
        var visible = VisibleColumns;
        float fixedTotal = visible.Where(c => c.Width > 0).Sum(c => c.Width);
        float flexWidth = global::System.Math.Max(60, totalWidth - fixedTotal);
        int flexCount = visible.Count(c => c.Width <= 0);
        float perFlex = flexCount > 0 ? flexWidth / flexCount : 0;

        var result = new List<(BreakdownColumn, float, float)>();
        float x = 0;

        foreach (var col in visible)
        {
            float w = col.Width > 0 ? col.Width : perFlex;
            result.Add((col, x, w));
            x += w;
        }

        return result;
    }
}
