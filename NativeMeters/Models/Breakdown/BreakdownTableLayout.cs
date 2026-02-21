using System.Collections.Generic;
using System.Linq;

namespace NativeMeters.Models.Breakdown;

public class BreakdownTableLayout
{
    public List<BreakdownColumn> Columns { get; set; } =
    [
        new("Action", "Action", 0, true, ColumnAlignment.Left),
        new("Hits", "Hits", 36),
        new("Crit", "C%", 40),
        new("DH", "DH%", 40),
        new("Pct", "%", 46),
        new("Total", "Damage", 64),
        new("PerSec", "DPS", 48),
        new("Max", "Max", 54),
        new("Active", "Active", 42),
    ];

    public IReadOnlyList<BreakdownColumn> VisibleColumns
        => Columns.Where(c => c.IsVisible).ToList();

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
