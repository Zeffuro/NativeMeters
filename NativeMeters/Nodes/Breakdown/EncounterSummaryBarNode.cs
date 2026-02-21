using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class EncounterSummaryBarNode : ResNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly TextNode totalDamageText;
    private readonly TextNode raidDpsText;
    private readonly TextNode durationText;
    private readonly TextNode deathsText;

    public EncounterSummaryBarNode()
    {
        totalDamageText = new TextNode
        {
            Position = new Vector2(8, 4),
            Size = new Vector2(160, 20),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        totalDamageText.AttachNode(this);

        raidDpsText = new TextNode
        {
            Position = new Vector2(170, 4),
            Size = new Vector2(140, 20),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        raidDpsText.AttachNode(this);

        durationText = new TextNode
        {
            Position = new Vector2(320, 4),
            Size = new Vector2(120, 20),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        durationText.AttachNode(this);

        deathsText = new TextNode
        {
            Position = new Vector2(440, 4),
            Size = new Vector2(110, 20),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = ColorHelper.GetColor(50),
        };
        deathsText.AttachNode(this);

        Size = new Vector2(560, 28);
    }

    public void Update(Encounter? encounter)
    {
        if (encounter == null)
        {
            totalDamageText.String = "No Data";
            raidDpsText.String = "";
            durationText.String = "";
            deathsText.String = "";
            return;
        }

        totalDamageText.String = $"Dmg: {Formatter.Format(encounter.Damage, "", "m", 2)}";
        raidDpsText.String = $"rDPS: {Formatter.Format(encounter.ENCDPS, "", "", 0)}";
        durationText.String = $"Dur: {encounter.Duration:mm\\:ss}";
        deathsText.String = $"Deaths: {encounter.Deaths}";
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (totalDamageText == null || raidDpsText == null || durationText == null || deathsText == null) return;

        float quarter = Width / 4f;
        totalDamageText.Size = new Vector2(quarter, 20);
        raidDpsText.Position = new Vector2(quarter, 4);
        raidDpsText.Size = new Vector2(quarter, 20);
        durationText.Position = new Vector2(quarter * 2, 4);
        durationText.Size = new Vector2(quarter, 20);
        deathsText.Position = new Vector2(quarter * 3, 4);
        deathsText.Size = new Vector2(quarter, 20);
    }
}
