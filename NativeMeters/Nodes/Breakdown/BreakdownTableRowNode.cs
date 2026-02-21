using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownTableRowNode : SimpleComponentNode
{
    private static readonly NumericFormatter Formatter = new();
    private static readonly Vector4 RowTextColor = new(0.9f, 0.9f, 0.9f, 1f);

    private readonly IconImageNode actionIcon;
    private readonly List<TextNode> cellTexts = new();

    private BreakdownTableLayout? layout;
    private const float RowHeight = 22f;
    private const float IconSize = 20f;

    public BreakdownTableRowNode()
    {
        actionIcon = new IconImageNode
        {
            Position = new Vector2(2, 1),
            Size = new Vector2(IconSize, IconSize),
            FitTexture = true,
        };
        actionIcon.AttachNode(this);

        Size = new Vector2(500, RowHeight);
    }

    public void SetLayout(BreakdownTableLayout tableLayout)
    {
        foreach (var t in cellTexts) t.Dispose();
        cellTexts.Clear();

        layout = tableLayout;

        foreach (var col in tableLayout.VisibleColumns)
        {
            var text = new TextNode
            {
                Size = new Vector2(col.Width > 0 ? col.Width : 60, RowHeight),
                FontSize = 12,
                FontType = FontType.Axis,
                TextFlags = TextFlags.Edge,
                AlignmentType = col.Alignment switch
                {
                    ColumnAlignment.Left => AlignmentType.Left,
                    ColumnAlignment.Center => AlignmentType.Center,
                    _ => AlignmentType.Right,
                },
                TextColor = RowTextColor,
            };
            text.AttachNode(this);
            cellTexts.Add(text);
        }

        RepositionCells();
    }

    public void SetData(ActionStatView action, bool isDamageMode)
    {
        if (layout == null) return;

        actionIcon.IsVisible = action.ActionIconId > 0;
        if (action.ActionIconId > 0) actionIcon.IconId = action.ActionIconId;

        var visible = layout.VisibleColumns;
        for (int i = 0; i < visible.Count && i < cellTexts.Count; i++)
        {
            cellTexts[i].String = visible[i].Key switch
            {
                "Action" => action.ActionName,
                "Hits" => action.Hits.ToString(),
                "Crit" => action.Hits > 0 ? $"{action.CritHits * 100.0 / action.Hits:F1}%" : "0%",
                "DH" => action.Hits > 0 ? $"{action.DirectHits * 100.0 / action.Hits:F1}%" : "0%",
                "Total" => isDamageMode
                    ? Formatter.Format(action.TotalDamage, "", "k", 1)
                    : Formatter.Format(action.TotalHealing, "", "k", 1),
                "PerSec" => isDamageMode
                    ? (action.DamagePerSecond > 0 ? Formatter.Format(action.DamagePerSecond, "", "", 0) : "0")
                    : (action.HealingPerSecond > 0 ? Formatter.Format(action.HealingPerSecond, "", "", 0) : "0"),
                "Max" => action.MaxHit > 0 ? Formatter.Format(action.MaxHit, "", "k", 1) : "-",
                "Active" => action.ActiveSpan.TotalSeconds > 0 ? $"{action.ActiveSpan:m\\:ss}" : "-",
                _ => "",
            };
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        RepositionCells();
    }

    private void RepositionCells()
    {
        if (layout == null || cellTexts.Count == 0) return;

        var resolved = layout.Resolve(Width);

        for (int i = 0; i < resolved.Count && i < cellTexts.Count; i++)
        {
            var (col, x, w) = resolved[i];

            // For the Action column, offset for the icon
            if (col.Key == "Action")
            {
                float iconOffset = IconSize + 6;
                cellTexts[i].Position = new Vector2(x + iconOffset, 1);
                cellTexts[i].Size = new Vector2(Math.Max(20, w - iconOffset), RowHeight);
                actionIcon.Position = new Vector2(x + 2, 1);
            }
            else
            {
                cellTexts[i].Position = new Vector2(x, 1);
                cellTexts[i].Size = new Vector2(w, RowHeight);
            }
        }
    }
}
