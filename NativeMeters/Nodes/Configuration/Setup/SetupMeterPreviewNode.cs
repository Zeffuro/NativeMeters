using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupMeterPreviewNode(SetupSampleFight sample) : ResNode
{
    private const float WindowBackgroundInset = 8;
    private readonly Dictionary<Func<Combatant, double>, double> maxima = [];
    private ResNode? content;

    public void ShowMeter(MeterSettings meter, int rowCount)
    {
        meter.IsClickthrough = true;
        var rows = MeterRowBuilder.Build(new List<Combatant>(sample.Members), meter, preview: true).Expanded;
        if (rows.Count > rowCount) rows.RemoveRange(rowCount, rows.Count - rowCount);
        var horizontal = meter.Layout == MeterLayout.Horizontal;
        var inset = meter.ShowWindowBackground && meter.BackgroundStyle is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused
            ? WindowBackgroundInset : 0f;
        var available = meter.Size.X - inset * 2;
        var rowWidth = horizontal ? MeterRowBuilder.GetHorizontalCardWidth(available, meter) : available;
        var gap = horizontal ? MeterRowBuilder.GetHorizontalCardSpacing(available, meter) : meter.RowSpacing;
        var headerHeight = !horizontal && meter.HeaderEnabled ? meter.HeaderHeight : 0;
        var footerHeight = meter.FooterEnabled ? meter.FooterHeight : 0;
        var width = horizontal ? rows.Count * rowWidth + Math.Max(0, rows.Count - 1) * gap + inset * 2 : meter.Size.X;
        var rowsHeight = horizontal ? meter.RowHeight : rows.Count * meter.RowHeight + Math.Max(0, rows.Count - 1) * gap;
        var height = headerHeight + rowsHeight + footerHeight + inset * 2;
        var scale = Math.Min(1, Math.Min(Width / Math.Max(1, width), Height / Math.Max(1, height)));
        Func<Func<Combatant, double>, double> maxStat = GetMaxStat;

        var replacement = new ResNode { Size = new Vector2(width, height), Scale = new Vector2(scale) };
        try
        {
            if (meter.ShowWindowBackground)
            {
                new MeterBackgroundNode
                {
                    Size = replacement.Size,
                    Style = meter.BackgroundStyle,
                    BackgroundColor = meter.WindowColor,
                }.AttachNode(replacement);
            }

            AddSummary(meter.HeaderComponents, headerHeight, inset, inset);
            for (var i = 0; i < rows.Count; i++)
            {
                var node = new MeterRowListItemNode
                {
                    Position = horizontal
                        ? new Vector2(inset + i * (rowWidth + gap), inset)
                        : new Vector2(inset, inset + headerHeight + i * (meter.RowHeight + gap)),
                    Size = new Vector2(rowWidth, meter.RowHeight),
                    ItemData = rows[i],
                    PreviewMaxStat = maxStat,
                };
                node.AttachNode(replacement);
                node.Update();
            }
            AddSummary(meter.FooterComponents, footerHeight, inset, inset + headerHeight + rowsHeight);

            content?.Dispose();
            content = null;
            replacement.AttachNode(this);
            content = replacement;
        }
        catch
        {
            replacement.Dispose();
            throw;
        }

        void AddSummary(List<ComponentSettings> components, float summaryHeight, float x, float y)
        {
            if (summaryHeight <= 0) return;
            var node = new StaticComponentContainerNode(components.Where(component => component.Type != MeterComponentType.MenuButton).ToList())
            {
                MeterSettings = meter,
                Position = new Vector2(x, y),
                Size = new Vector2(available, summaryHeight),
            };
            node.AttachNode(replacement);
            node.Update(sample.Encounter);
        }
    }

    private double GetMaxStat(Func<Combatant, double> selector)
    {
        if (maxima.TryGetValue(selector, out var max)) return max;
        max = sample.Members.Max(selector);
        maxima.Add(selector, max);
        return max;
    }
}
