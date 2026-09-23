using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace NativeMeters.Services.Internal;

internal sealed class LevelPotency
{
    private readonly record struct Key(uint ActionId, uint JobId, byte Level, bool Periodic, bool Ground);
    private readonly Dictionary<Key, decimal?> cache = new();
    private readonly ExcelSheet<ActionTransient> descriptions = Service.DataManager.GetExcelSheet<ActionTransient>(ClientLanguage.English);
    private static readonly Regex PotencyOf = new(@"\bpotency of ([\d,]+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex DotPotency = new(@"\b(?:DoT|Damage over Time) Potency:\s*([\d,]+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PotencyLine = new(@"^Potency:\s*([\d,]+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex Conditional = new(@"\b(combo|flank|rear|maximum|varies|increased|increases|up to)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public decimal? Get(uint actionId, uint jobId, byte level, bool periodic, bool ground = false)
    {
        if (actionId == 0 || level == 0) return null;

        var key = new Key(actionId, jobId, level, periodic, ground);
        if (cache.TryGetValue(key, out var potency)) return potency;

        var row = descriptions.GetRowOrDefault(actionId);
        var text = row.HasValue ? new ActionTooltipText(jobId, level).Read(row.Value.Description) : null;
        potency = text == null ? null : Parse(text, periodic, ground);
        cache[key] = potency;
        return potency;
    }

    private static decimal? Parse(string text, bool periodic, bool ground)
    {
        if (!periodic)
        {
            if (Conditional.IsMatch(text)) return null;
            var matches = PotencyOf.Matches(text);
            return matches.Count == 1 ? Number(matches[0]) : null;
        }

        var explicitPotencies = DotPotency.Matches(text);
        if (explicitPotencies.Count == 1) return Number(explicitPotencies[0]);
        if (explicitPotencies.Count > 1) return null;

        decimal? result = null;
        var damageOverTime = false;
        foreach (var line in text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains("damage over time", StringComparison.OrdinalIgnoreCase)) damageOverTime = true;
            if (!damageOverTime) continue;

            var match = PotencyLine.Match(line);
            if (!match.Success) continue;
            if (result != null) return null;
            result = Number(match);
        }

        if (result != null || !ground) return result;

        var groundPotencies = PotencyOf.Matches(text);
        return groundPotencies.Count == 1 ? Number(groundPotencies[0]) : null;
    }

    private static decimal? Number(Match match)
        => decimal.TryParse(match.Groups[1].Value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value : null;
}
