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

    private static readonly Vector4 PrimaryColor = new(1f, 1f, 1f, 1f);
    private static readonly Vector4 NormalColor = new(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Vector4 DimmedColor = new(0.82f, 0.82f, 0.82f, 1f);

    private readonly SimpleNineGridNode contributionBar;
    private readonly IconImageNode actionIcon;
    private readonly List<TextNode> cellTexts = new();

    private BreakdownTableLayout? layout;
    private bool layoutInitialized;

    public const float RowHeight = 26f;
    private const float IconSize = 22f;
    public const float ActionTextOffset = 30f;

    public BreakdownTableRowNode()
    {
        contributionBar = new SimpleNineGridNode
        {
            TexturePath = "ui/uld/ToolTipS.tex",
            TextureCoordinates = Vector2.Zero,
            TextureSize = new Vector2(32.0f, 24.0f),
            TopOffset = 10,
            BottomOffset = 10,
            LeftOffset = 15,
            RightOffset = 15,
            Position = new Vector2(0, 0),
            Size = new Vector2(0, RowHeight),
            Color = new Vector4(0.85f, 0.85f, 0.95f, 0.25f),
            AddColor = new Vector3(1f, 1f, 1f),
            IsVisible = false,
        };
        contributionBar.AttachNode(this);

        actionIcon = new IconImageNode
        {
            Position = new Vector2(2, (RowHeight - IconSize) / 2),
            Size = new Vector2(IconSize, IconSize),
            FitTexture = true,
        };
        actionIcon.AttachNode(this);

        Size = new Vector2(500, RowHeight);
    }

    public void SetLayout(BreakdownTableLayout tableLayout)
    {
        if (layoutInitialized && layout == tableLayout) return;

        if (layoutInitialized)
        {
            foreach (var t in cellTexts) t.Dispose();
            cellTexts.Clear();
        }

        layout = tableLayout;
        layoutInitialized = true;

        foreach (var col in tableLayout.VisibleColumns)
        {
            var textColor = col.Key switch
            {
                "Total" => PrimaryColor,
                "Action" or "Pct" => NormalColor,
                _ => DimmedColor,
            };

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
                TextColor = textColor,
            };
            text.AttachNode(this);
            cellTexts.Add(text);
        }

        RepositionCells();
    }

    public void SetData(ActionStatView action, bool isDamageMode, double contributionPct = 0)
    {
        if (layout == null) return;

        var actionName = action.ActionName;
        var actionIconId = action.ActionIconId;

        if (actionName.Equals("attack", StringComparison.OrdinalIgnoreCase))
        {
            actionName = "Attack";
            actionIconId = 101;
        }

        actionIcon.IsVisible = actionIconId > 0;
        if (actionIconId > 0) actionIcon.IconId = actionIconId;

        if (contributionPct > 0)
        {
            contributionBar.IsVisible = true;
            float barWidth = (float)(Math.Min(1.0, contributionPct / 100.0) * Width);
            contributionBar.Size = new Vector2(Math.Max(4, barWidth), RowHeight);
        }
        else
        {
            contributionBar.IsVisible = false;
        }

        var visible = layout.VisibleColumns;
        for (int i = 0; i < visible.Count && i < cellTexts.Count; i++)
        {
            cellTexts[i].String = visible[i].Key switch
            {
                "Action" => actionName,
                "Hits" => action.Hits.ToString(),
                "Crit" => action.Hits > 0 ? $"{(int)Math.Round(action.CritHits * 100.0 / action.Hits)}%" : "-",
                "DH" => action.Hits > 0 ? $"{(int)Math.Round(action.DirectHits * 100.0 / action.Hits)}%" : "-",
                "Pct" => isDamageMode
                    ? (action.DamagePercent > 0 ? $"{action.DamagePercent:F1}%" : "-")
                    : (action.HealingPercent > 0 ? $"{action.HealingPercent:F1}%" : "-"),
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

        contributionBar.Position = Vector2.Zero;

        var resolved = layout.Resolve(Width);

        for (int i = 0; i < resolved.Count && i < cellTexts.Count; i++)
        {
            var (col, x, w) = resolved[i];

            if (col.Key == "Action")
            {
                cellTexts[i].Position = new Vector2(x + ActionTextOffset, 0);
                cellTexts[i].Size = new Vector2(Math.Max(20, w - ActionTextOffset), RowHeight);
                actionIcon.Position = new Vector2(x + 2, (RowHeight - IconSize) / 2);
            }
            else
            {
                cellTexts[i].Position = new Vector2(x, 0);
                cellTexts[i].Size = new Vector2(w, RowHeight);
            }
        }
    }
}
