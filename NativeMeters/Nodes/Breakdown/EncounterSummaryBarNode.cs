using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class EncounterSummaryBarNode : SimpleComponentNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly TextNode primaryValueText;
    private readonly TextNode secondaryValueText;
    private readonly TextNode durationText;

    private const float TextY = 4f;
    private const float TextHeight = 20f;

    public EncounterSummaryBarNode()
    {
        primaryValueText = new TextNode
        {
            Position = new Vector2(8, TextY),
            Size = new Vector2(140, TextHeight),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        primaryValueText.AttachNode(this);

        secondaryValueText = new TextNode
        {
            Position = new Vector2(0, TextY),
            Size = new Vector2(120, TextHeight),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Center,
            TextColor = new Vector4(0.85f, 0.85f, 0.85f, 1f),
        };
        secondaryValueText.AttachNode(this);

        durationText = new TextNode
        {
            Position = new Vector2(0, TextY),
            Size = new Vector2(120, TextHeight),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.65f, 0.65f, 0.65f, 1f),
        };
        durationText.AttachNode(this);

        Size = new Vector2(560, 28);
    }

    public void Update(Encounter? encounter, BreakdownTab tab = BreakdownTab.Damage)
    {
        if (encounter == null)
        {
            primaryValueText.String = "No Data";
            secondaryValueText.String = "";
            durationText.String = "";
            return;
        }

        switch (tab)
        {
            case BreakdownTab.Damage:
                primaryValueText.String = $"{Formatter.Format(encounter.Damage, "", "m", 2)} Total";
                secondaryValueText.String = $"{Formatter.Format(encounter.ENCDPS, "", "", 0)} rDPS";
                break;
            case BreakdownTab.Healing:
                primaryValueText.String = $"{Formatter.Format(encounter.Healed, "", "m", 2)} Healed";
                secondaryValueText.String = $"{Formatter.Format(encounter.ENCHPS, "", "", 0)} rHPS";
                break;
            case BreakdownTab.DamageTaken:
                primaryValueText.String = $"{Formatter.Format(encounter.Damagetaken, "", "m", 2)} Taken";
                secondaryValueText.String = encounter.DURATION > 0
                    ? $"{Formatter.Format(encounter.Damagetaken / encounter.DURATION, "", "", 0)} DTPS"
                    : "0 DTPS";
                break;
        }

        durationText.String = $"{encounter.Duration:mm\\:ss}";
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (primaryValueText == null) return;

        float third = Width / 3f;
        primaryValueText.Position = new Vector2(8, TextY);
        primaryValueText.Size = new Vector2(third - 8, TextHeight);

        secondaryValueText.Position = new Vector2(third, TextY);
        secondaryValueText.Size = new Vector2(third, TextHeight);

        durationText.Position = new Vector2(third * 2, TextY);
        durationText.Size = new Vector2(third - 8, TextHeight);
    }
}
