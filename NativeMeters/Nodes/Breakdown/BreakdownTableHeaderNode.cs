using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Models.Breakdown;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownTableHeaderNode : SimpleComponentNode
{
    private static readonly Vector4 HeaderColor = new(0.6f, 0.6f, 0.6f, 1f);
    private const float TextHeight = 14f;
    private const float SeparatorHeight = 2f;
    private const float TotalHeight = TextHeight + SeparatorHeight + 2f;

    private readonly List<TextNode> headerTexts = new();
    private readonly HorizontalLineNode separatorLine;
    private BreakdownTableLayout? layout;

    public BreakdownTableHeaderNode()
    {
        separatorLine = new HorizontalLineNode
        {
            Position = new Vector2(0, TextHeight + 1),
            Size = new Vector2(500, SeparatorHeight),
            Color = new Vector4(1f, 1f, 1f, 0.3f),
            IsVisible = true,
        };
        separatorLine.AttachNode(this);

        Size = new Vector2(500, TotalHeight);
    }

    public void SetLayout(BreakdownTableLayout tableLayout)
    {
        foreach (var t in headerTexts) t.Dispose();
        headerTexts.Clear();

        layout = tableLayout;

        foreach (var col in tableLayout.VisibleColumns)
        {
            var text = new TextNode
            {
                Size = new Vector2(col.Width > 0 ? col.Width : 60, TextHeight),
                FontSize = 11,
                FontType = FontType.Axis,
                AlignmentType = col.Alignment switch
                {
                    ColumnAlignment.Left => AlignmentType.Left,
                    ColumnAlignment.Center => AlignmentType.Center,
                    _ => AlignmentType.Right,
                },
                TextColor = HeaderColor,
                String = col.Label,
            };
            text.AttachNode(this);
            headerTexts.Add(text);
        }

        RepositionHeaders();
    }

    public void UpdateLabels(string totalLabel, string perSecLabel)
    {
        if (layout == null) return;

        var visible = layout.VisibleColumns;
        for (int i = 0; i < visible.Count && i < headerTexts.Count; i++)
        {
            if (visible[i].Key == "Total") headerTexts[i].String = totalLabel;
            if (visible[i].Key == "PerSec") headerTexts[i].String = perSecLabel;
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        RepositionHeaders();
    }

    private void RepositionHeaders()
    {
        if (layout == null || headerTexts.Count == 0) return;

        separatorLine.Size = new Vector2(Width, SeparatorHeight);

        var resolved = layout.Resolve(Width);
        for (int i = 0; i < resolved.Count && i < headerTexts.Count; i++)
        {
            var (_, x, w) = resolved[i];
            headerTexts[i].Position = new Vector2(x, 0);
            headerTexts[i].Size = new Vector2(w, TextHeight);
        }
    }
}
