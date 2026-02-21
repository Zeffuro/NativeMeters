using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class ActionBreakdownRowNode : SimpleComponentNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly IconImageNode actionIcon;
    private readonly TextNode actionNameText;
    private readonly TextNode hitsText;
    private readonly TextNode critPctText;
    private readonly TextNode dhPctText;
    private readonly TextNode totalText;
    private readonly TextNode perSecondText;
    private readonly TextNode maxHitText;
    private readonly TextNode activeTimeText;

    private const float RowHeight = 22f;
    private const float IconSize = 20f;
    private const float IconPadding = 4f;

    public ActionBreakdownRowNode()
    {
        actionIcon = new IconImageNode
        {
            Position = new Vector2(2, 1),
            Size = new Vector2(IconSize, IconSize),
            FitTexture = true,
        };
        actionIcon.AttachNode(this);

        actionNameText = new TextNode
        {
            Position = new Vector2(IconSize + IconPadding + 4, 1),
            Size = new Vector2(140, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        actionNameText.AttachNode(this);

        hitsText = new TextNode
        {
            Size = new Vector2(36, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        hitsText.AttachNode(this);

        critPctText = new TextNode
        {
            Size = new Vector2(48, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        critPctText.AttachNode(this);

        dhPctText = new TextNode
        {
            Size = new Vector2(48, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        dhPctText.AttachNode(this);

        totalText = new TextNode
        {
            Size = new Vector2(68, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        totalText.AttachNode(this);

        perSecondText = new TextNode
        {
            Size = new Vector2(56, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        perSecondText.AttachNode(this);

        maxHitText = new TextNode
        {
            Size = new Vector2(60, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        maxHitText.AttachNode(this);

        activeTimeText = new TextNode
        {
            Size = new Vector2(44, RowHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.9f, 0.9f, 0.9f, 1f),
        };
        activeTimeText.AttachNode(this);

        Size = new Vector2(500, RowHeight);
    }

    public void SetData(ActionStatView action, bool isDamageMode)
    {
        if (action.ActionIconId > 0)
        {
            actionIcon.IsVisible = true;
            actionIcon.IconId = action.ActionIconId;
        }
        else
        {
            actionIcon.IsVisible = false;
        }

        actionNameText.String = TruncateName(action.ActionName, 18);
        hitsText.String = action.Hits.ToString();
        critPctText.String = action.Hits > 0 ? $"{action.CritHits * 100.0 / action.Hits:F1}%" : "0%";
        dhPctText.String = action.Hits > 0 ? $"{action.DirectHits * 100.0 / action.Hits:F1}%" : "0%";

        if (isDamageMode)
        {
            totalText.String = Formatter.Format(action.TotalDamage, "", "m", 1);
            perSecondText.String = action.DamagePerSecond > 0 ? Formatter.Format(action.DamagePerSecond, "", "", 0) : "0";
        }
        else
        {
            totalText.String = Formatter.Format(action.TotalHealing, "", "m", 1);
            perSecondText.String = action.HealingPerSecond > 0 ? Formatter.Format(action.HealingPerSecond, "", "", 0) : "0";
        }

        maxHitText.String = action.MaxHit > 0 ? Formatter.Format(action.MaxHit, "", "m", 1) : "-";

        activeTimeText.String = action.ActiveSpan.TotalSeconds > 0
            ? $"{action.ActiveSpan:m\\:ss}"
            : "-";
    }

    private static string TruncateName(string name, int maxLen)
        => name.Length <= maxLen ? name : name[..(maxLen - 1)] + "…";

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (actionNameText == null || hitsText == null || maxHitText == null) return;
        RepositionColumns();
    }

    private void RepositionColumns()
    {
        float x = Width;

        float maxHitW = 60f; x -= maxHitW;
        maxHitText.Position = new Vector2(x, 1);
        maxHitText.Size = new Vector2(maxHitW, RowHeight);

        float psW = 56f; x -= psW;
        perSecondText.Position = new Vector2(x, 1);
        perSecondText.Size = new Vector2(psW, RowHeight);

        float totalW = 68f; x -= totalW;
        totalText.Position = new Vector2(x, 1);
        totalText.Size = new Vector2(totalW, RowHeight);

        float dhW = 48f; x -= dhW;
        dhPctText.Position = new Vector2(x, 1);
        dhPctText.Size = new Vector2(dhW, RowHeight);

        float critW = 48f; x -= critW;
        critPctText.Position = new Vector2(x, 1);
        critPctText.Size = new Vector2(critW, RowHeight);

        float hitsW = 36f; x -= hitsW;
        hitsText.Position = new Vector2(x, 1);
        hitsText.Size = new Vector2(hitsW, RowHeight);

        float nameStart = IconSize + IconPadding + 4;
        float nameW = Math.Max(60, x - nameStart);
        actionNameText.Position = new Vector2(nameStart, 1);
        actionNameText.Size = new Vector2(nameW, RowHeight);
    }
}
