using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using Lumina.Data.Parsing.Uld;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterListItemNode : ListItemWithFocusNav<MeterSettings>, IListItemNode
{
    public static float ItemHeight => 48.0f;

    private readonly IconImageNode iconNode;
    private readonly TextNode labelTextNode;
    private readonly TextNode subLabelTextNode;

    public MeterListItemNode()
    {
        iconNode = new IconImageNode
        {
            FitTexture = true,
            IsVisible = false,
        };
        iconNode.AttachNode(this);

        labelTextNode = new TextNode
        {
            AlignmentType = AlignmentType.BottomLeft,
            FontSize = 14,
            TextFlags = TextFlags.Ellipsis | TextFlags.Emboss,
            TextColor = ColorHelper.GetColor(8),
            TextOutlineColor = ColorHelper.GetColor(7),
        };
        labelTextNode.AttachNode(this);

        subLabelTextNode = new TextNode
        {
            AlignmentType = AlignmentType.TopLeft,
            FontSize = 12,
            TextFlags = TextFlags.Ellipsis | TextFlags.Emboss,
            TextColor = ColorHelper.GetColor(3),
            TextOutlineColor = ColorHelper.GetColor(7),
        };
        subLabelTextNode.AttachNode(this);
    }

    protected override void SetNodeData(MeterSettings data)
    {
        var iconId = GetIconId(data);
        iconNode.IsVisible = iconId is > 0;
        if (iconId is > 0)
        {
            iconNode.IconId = iconId.Value;
        }

        labelTextNode.String = GetLabel(data);
        subLabelTextNode.String = GetSubLabel(data);

        UpdateLayout();
    }

    public static string GetLabel(MeterSettings settings)
        => string.IsNullOrWhiteSpace(settings.Name) ? "Unnamed Meter" : settings.Name;

    public static string GetSubLabel(MeterSettings settings)
    {
        var status = settings.IsEnabled ? "Enabled" : "Disabled";
        return $"{status} ({settings.StatToTrack})";
    }

    public static uint? GetIconId(MeterSettings settings) => 0;

    public static int Compare(MeterSettings left, MeterSettings right)
        => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        var iconSize = Math.Max(0.0f, Height - 8.0f);
        iconNode.Size = new Vector2(iconSize, iconSize);
        iconNode.Position = new Vector2(4.0f, 4.0f);

        var textX = iconNode.IsVisible ? iconNode.Bounds.Right + 4.0f : 8.0f;
        var textWidth = Math.Max(0.0f, Width - textX - 8.0f);

        labelTextNode.Size = new Vector2(textWidth, Height / 2.0f);
        labelTextNode.Position = new Vector2(textX, 0.0f);

        subLabelTextNode.Size = labelTextNode.Size;
        subLabelTextNode.Position = labelTextNode.Position with { Y = Height / 2.0f };
    }
}
