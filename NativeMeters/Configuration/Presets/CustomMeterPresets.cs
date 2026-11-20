using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Data.Stats;
using NativeMeters.Models;
using static NativeMeters.Configuration.Presets.MeterPresetComponents;

namespace NativeMeters.Configuration.Presets;

public enum CustomMeterBarStyle { None, Underline, FullWidth }

public enum CustomMeterColumn
{
    PlayerName,
    Rank,
    DamagePercent,
    Dps,
    Damage,
    Hps,
    Swings,
    Hits,
    DirectHitPercent,
    CriticalHitPercent,
    CriticalDirectHitPercent,
    BestHit,
    Deaths,
}

public sealed class CustomMeterOptions
{
    public MeterLayout Layout { get; set; } = MeterLayout.Vertical;
    public CustomMeterBarStyle BarStyle { get; set; } = CustomMeterBarStyle.FullWidth;
    public ProgressBarType? BarType { get; set; } = ProgressBarType.Cast;
    public ProgressBarSizing BarSizing { get; set; } = ProgressBarSizing.PreserveEnds;
    public bool? ShowBackground { get; set; }
    public ColorMode? PlayerColor { get; set; }
    public JobIconType IconStyle { get; set; } = JobIconType.Default;
    public ColorMode? BarColor { get; set; }
    public ProgressBarColorTreatment? BarTint { get; set; } = ProgressBarColorTreatment.Auto;
    public string SortBy { get; set; } = StatSelector.DefaultStatSelector;
    public string FillStat { get; set; } = StatSelector.DefaultStatSelector;
    public bool TextAboveBar { get; set; } = true;
    public bool ShowFooter { get; set; }
    public List<CustomMeterColumn> Columns { get; } =
        [CustomMeterColumn.PlayerName, CustomMeterColumn.DamagePercent, CustomMeterColumn.Dps];
}

public static class CustomMeterPresets
{
    private const float FilledBarHeight = 32f;
    private const float SeparateBarHeight = 16f;
    private const float CellHeight = 18f;
    private const float VerticalBottomPadding = 1f;
    private const float CardBottomPadding = 7f;

    private sealed record Column(CustomMeterColumn Id, string Label, string Heading, string Tag, float Width);

    private static readonly Column[] Definitions =
    [
        new(CustomMeterColumn.PlayerName, "Player Name", "Name", "[name]", 166),
        new(CustomMeterColumn.Rank, "Rank", "Rank", "[rank]", 38),
        new(CustomMeterColumn.DamagePercent, "Damage Share", "Dmg %", "[damagepct.1]%", 64),
        new(CustomMeterColumn.Dps, "DPS", "DPS", "[ENCDPS.0]", 74),
        new(CustomMeterColumn.Damage, "Total Damage", "Damage", "[damage.0]", 94),
        new(CustomMeterColumn.Hps, "HPS", "HPS", "[ENCHPS.0]", 74),
        new(CustomMeterColumn.Swings, "Swings", "Swings", "[swings.0]", 52),
        new(CustomMeterColumn.Hits, "Hits", "Hits", "[hits.0]", 50),
        new(CustomMeterColumn.DirectHitPercent, "Direct Hit Rate", "DH %", "[directhitpct.0]%", 48),
        new(CustomMeterColumn.CriticalHitPercent, "Critical Hit Rate", "CH %", "[crithitpct.0]%", 48),
        new(CustomMeterColumn.CriticalDirectHitPercent, "Critical Direct Hit Rate", "CDH %", "[critdirecthitpct.0]%", 54),
        new(CustomMeterColumn.BestHit, "Best Hit", "Best Hit", "[maxhit_val.0]", 82),
        new(CustomMeterColumn.Deaths, "Deaths", "Deaths", "[deaths.0]", 54),
    ];

    public static IReadOnlyList<(CustomMeterColumn Column, string Label)> Columns { get; } =
        Array.AsReadOnly(Definitions.Select(column => (column.Id, column.Label)).ToArray());

    public static IReadOnlyList<(CustomMeterBarStyle Style, string Label)> BarStyles { get; } =
        Array.AsReadOnly<(CustomMeterBarStyle, string)>(
        [
            (CustomMeterBarStyle.None, "No Bar"),
            (CustomMeterBarStyle.Underline, "Underline"),
            (CustomMeterBarStyle.FullWidth, "Filled Bar"),
        ]);

    public static void Apply(CustomMeterOptions options, MeterSettings target)
    {
        var columns = ResolveColumns(options.Columns);
        var filled = options.BarStyle == CustomMeterBarStyle.FullWidth;
        var textAbove = filled && options.TextAboveBar;
        var filledHeight = textAbove
            ? Math.Max(SeparateBarHeight, GetMinimumBarHeight(options.BarType ?? ProgressBarType.ToDo))
            : FilledBarHeight;
        if (options.Layout == MeterLayout.Horizontal)
            ApplyHorizontal(target, columns, filled, textAbove, filledHeight);
        else
            ApplyVertical(target, columns, showTotals: false, filled, textAbove, filledHeight);
        var bar = ApplyAppearance(target, options.BarStyle, options.ShowBackground, options.BarType, options.PlayerColor,
            options.BarTint, options.BarColor, options.SortBy, options.FillStat, options.IconStyle, options.BarSizing);
        if (filled && bar != null)
        {
            var horizontal = options.Layout == MeterLayout.Horizontal;
            var bottomPadding = horizontal ? CardBottomPadding : VerticalBottomPadding;
            bar.Position = bar.Position with { Y = textAbove ? target.RowHeight - filledHeight - bottomPadding : 1f };
            bar.Size = bar.Size with { Y = filledHeight };
        }
        if (options.ShowFooter) AddFooter(target);
    }

    private static void AddFooter(MeterSettings target)
    {
        const float height = 24f;
        target.FooterEnabled = true;
        target.FooterHeight = height;
        target.FooterComponents =
        [
            Text("Deaths", "Deaths: [deaths]", 8, 2, 92, 20),
            Text("Party DPS", "Party DPS: [ENCDPS.0]", 108, 2, 152, 20),
        ];
        target.Size = target.Size with { Y = target.Size.Y + height };
    }

    internal static void ApplyVertical(
        MeterSettings target, CustomMeterBarStyle style, IEnumerable<CustomMeterColumn> selection, bool showTotals)
    {
        ApplyVertical(target, ResolveColumns(selection), showTotals);
        ApplyAppearance(target, style, null);
    }

    private static Column[] ResolveColumns(IEnumerable<CustomMeterColumn> selection)
    {
        var columns = selection
            .Distinct()
            .Select(id => Array.Find(Definitions, column => column.Id == id))
            .OfType<Column>()
            .ToArray();
        return columns.Length == 0 ? [Definitions[0]] : columns;
    }

    private static void ApplyVertical(
        MeterSettings target, IReadOnlyList<Column> columns, bool showTotals, bool filled = false,
        bool textAbove = false, float filledHeight = FilledBarHeight)
    {
        const float headerHeight = 50f;
        const float leftPad = 8f;
        const float gap = 8f;
        var rowHeight = filled ? filledHeight + 2f : 22f;
        if (textAbove) rowHeight += CellHeight;
        var textY = filled && !textAbove ? (rowHeight - CellHeight) / 2f : 0f;
        var showJob = columns.Any(column => column.Id == CustomMeterColumn.PlayerName);
        var startX = showJob ? 34f : leftPad;
        var minimumWidth = showTotals ? 532f : 268f;
        var width = Math.Max(minimumWidth, startX + columns.Sum(column => column.Width + gap));

        var headers = Header(width, showTotals);

        List<ComponentSettings> rows = [];
        if (showJob) rows.Add(Job(leftPad, filled ? textY : 1f, CellHeight));

        var x = startX;
        foreach (var column in columns)
        {
            var isName = column.Id == CustomMeterColumn.PlayerName;
            var cellWidth = column.Width;
            var align = isName ? AlignmentType.Left : AlignmentType.Right;
            headers.Add(Text(column.Heading, column.Heading, x, 29, cellWidth, 17, 12, align, Accent));
            rows.Add(Text(column.Label, column.Tag, x, textY, cellWidth, CellHeight, 14, align));
            x += cellWidth + gap;
        }

        SetLayout(target, MeterLayout.Vertical, width, rowHeight, 2, headerHeight, headers, rows);
    }

    private static void ApplyHorizontal(MeterSettings target, IReadOnlyList<Column> columns, bool filled,
        bool textAbove, float filledHeight)
    {
        const float cardWidth = 188f;
        const float cardSpacing = 5f;
        const float horizontalPadding = 8f;
        const float cellGap = 8f;
        const float cellWidth = (cardWidth - horizontalPadding * 2 - cellGap) / 2;
        const float headerHeight = 28f;
        const float horizontalWidth = cardWidth * DefaultCombatants + cardSpacing * (DefaultCombatants - 1);

        List<ComponentSettings> rows = [];
        var textInside = filled && !textAbove;
        var y = textInside ? 1f + (filledHeight - CellHeight) / 2f : 4f;
        var firstLineY = y;
        var leftCell = true;
        foreach (var column in columns)
        {
            var isName = column.Id == CustomMeterColumn.PlayerName;
            if (isName || column.Id == CustomMeterColumn.BestHit)
            {
                if (!leftCell)
                {
                    y = NextLine(y);
                    leftCell = true;
                }

                if (isName) rows.Add(Job(horizontalPadding, y, 17));
                var x = isName ? 30f : horizontalPadding;
                var tag = isName ? column.Tag : "Best [maxhit_val:c.1]";
                rows.Add(Text(column.Label, tag, x, y, cardWidth - x - horizontalPadding, CellHeight,
                    isName ? 14u : 12u));
                y = NextLine(y);
                continue;
            }

            var cellX = leftCell ? horizontalPadding : horizontalPadding + cellWidth + cellGap;
            rows.Add(Text(column.Label, HorizontalStatText(column), cellX, y, cellWidth, CellHeight, 12));
            if (!leftCell) y = NextLine(y);
            leftCell = !leftCell;
        }

        var contentBottom = rows.Max(component => component.Position.Y + component.Size.Y);
        if (textAbove) contentBottom += filledHeight + 1f;
        var height = Math.Max(filled ? filledHeight + 2f : 31f, contentBottom + CardBottomPadding);
        rows.Insert(0, Background(cardWidth, height, CardTint));
        SetLayout(target, MeterLayout.Horizontal, cardWidth, height, cardSpacing, headerHeight,
            Header(horizontalWidth, totals: false), rows);

        float NextLine(float currentY)
            => textInside && currentY == firstLineY ? filledHeight + 3f : currentY + CellHeight;
    }

    private static string HorizontalStatText(Column column) => column.Id switch
    {
        CustomMeterColumn.Rank => "Rank [rank]",
        CustomMeterColumn.DamagePercent => "Share [damagepct.0]%",
        CustomMeterColumn.Dps => "DPS [ENCDPS:c.1]",
        CustomMeterColumn.Damage => "Dmg [damage:c.1]",
        CustomMeterColumn.Hps => "HPS [ENCHPS:c.1]",
        CustomMeterColumn.Swings => "Swings [swings:c.0]",
        CustomMeterColumn.Hits => "Hits [hits:c.0]",
        _ => $"{column.Heading.TrimEnd(' ', '%')} {column.Tag}",
    };
}
