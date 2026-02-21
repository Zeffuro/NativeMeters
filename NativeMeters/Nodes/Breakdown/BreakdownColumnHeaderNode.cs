using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownColumnHeaderNode : SimpleComponentNode
{
    private readonly TextNode actionLabel;
    private readonly TextNode hitsLabel;
    private readonly TextNode critLabel;
    private readonly TextNode dhLabel;
    private readonly TextNode totalLabel;
    private readonly TextNode psLabel;
    private readonly TextNode maxLabel;
    private readonly TextNode activeLabel;

    private static readonly Vector4 HeaderColor = new(0.7f, 0.7f, 0.7f, 1f);
    private const float RowHeight = 18f;

    public BreakdownColumnHeaderNode()
    {
        actionLabel = new TextNode
        {
            Position = new Vector2(4, 0),
            Size = new Vector2(60, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Left,
            TextColor = HeaderColor,
            String = "Action",
        };
        actionLabel.AttachNode(this);

        hitsLabel = new TextNode
        {
            Size = new Vector2(36, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "Hits",
        };
        hitsLabel.AttachNode(this);

        critLabel = new TextNode
        {
            Size = new Vector2(48, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "Crit%",
        };
        critLabel.AttachNode(this);

        dhLabel = new TextNode
        {
            Size = new Vector2(48, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "DH%",
        };
        dhLabel.AttachNode(this);

        totalLabel = new TextNode
        {
            Size = new Vector2(68, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "Total",
        };
        totalLabel.AttachNode(this);

        psLabel = new TextNode
        {
            Size = new Vector2(56, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "/s",
        };
        psLabel.AttachNode(this);

        maxLabel = new TextNode
        {
            Size = new Vector2(60, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "Max",
        };
        maxLabel.AttachNode(this);

        activeLabel = new TextNode
        {
            Size = new Vector2(44, RowHeight),
            FontSize = 11,
            FontType = FontType.Axis,
            AlignmentType = AlignmentType.Right,
            TextColor = HeaderColor,
            String = "Active",
        };
        activeLabel.AttachNode(this);

        Size = new Vector2(500, RowHeight);
    }

    public void SetTotalLabel(string label) => totalLabel.String = label;
    public void SetPerSecondLabel(string label) => psLabel.String = label;

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (actionLabel == null || hitsLabel == null || maxLabel == null) return;

        float x = Width;

        float maxW = 60f; x -= maxW;
        maxLabel.Position = new Vector2(x, 0);
        maxLabel.Size = new Vector2(maxW, RowHeight);

        float psW = 56f; x -= psW;
        psLabel.Position = new Vector2(x, 0);
        psLabel.Size = new Vector2(psW, RowHeight);

        float totalW = 68f; x -= totalW;
        totalLabel.Position = new Vector2(x, 0);
        totalLabel.Size = new Vector2(totalW, RowHeight);

        float dhW = 48f; x -= dhW;
        dhLabel.Position = new Vector2(x, 0);
        dhLabel.Size = new Vector2(dhW, RowHeight);

        float critW = 48f; x -= critW;
        critLabel.Position = new Vector2(x, 0);
        critLabel.Size = new Vector2(critW, RowHeight);

        float hitsW = 36f; x -= hitsW;
        hitsLabel.Position = new Vector2(x, 0);
        hitsLabel.Size = new Vector2(hitsW, RowHeight);

        actionLabel.Size = new Vector2(Math.Max(60, x - 4), RowHeight);
    }
}
