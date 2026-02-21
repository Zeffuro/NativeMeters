using System.Collections.Generic;
using System.Linq;

namespace NativeMeters.Models.Breakdown;

public class BreakdownTableLayout
{
    public List<BreakdownColumn> Columns { get; set; } =
    [
        new("Action", "Action", 0, true, ColumnAlignment.Left),   // 0 = flex fill remaining
        new("Hits", "Hits", 32),
        new("Crit", "Crit%", 44),
        new("DH", "DH%", 44),
        new("Total", "Damage", 60),
        new("PerSec", "DPS", 50),
        new("Max", "Max", 52),
        new("Active", "Active", 40),
    ];

    public IReadOnlyList<BreakdownColumn> VisibleColumns
        => Columns.Where(c => c.IsVisible).ToList();

    /// <summary>
    /// Given the total available width, calculates (x, w) for each visible column.
    /// Columns with Width=0 are flex and fill the remaining space.
    /// </summary>
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
