using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class EncounterSummaryBarNode : SimpleComponentNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly HorizontalLineNode topLine;
    private readonly TextNode totalDamageText;
    private readonly TextNode raidDpsText;
    private readonly TextNode durationText;
    private readonly TextNode deathsText;
    private readonly HorizontalLineNode bottomLine;

    private const float LineHeight = 2f;
    private const float TextY = LineHeight + 4f;
    private const float TextHeight = 20f;

    public EncounterSummaryBarNode()
    {
        topLine = new HorizontalLineNode
        {
            Position = Vector2.Zero,
            Size = new Vector2(560, LineHeight),
            Color = new Vector4(1f, 1f, 1f, 0.25f),
            IsVisible = true,
        };
        topLine.AttachNode(this);

        totalDamageText = new TextNode
        {
            Position = new Vector2(8, TextY),
            Size = new Vector2(140, TextHeight),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        totalDamageText.AttachNode(this);

        raidDpsText = new TextNode
        {
            Position = new Vector2(150, TextY),
            Size = new Vector2(120, TextHeight),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        raidDpsText.AttachNode(this);

        durationText = new TextNode
        {
            Position = new Vector2(280, TextY),
            Size = new Vector2(120, TextHeight),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.75f, 0.75f, 0.75f, 1f),
        };
        durationText.AttachNode(this);

        deathsText = new TextNode
        {
            Position = new Vector2(400, TextY),
            Size = new Vector2(110, TextHeight),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.75f, 0.75f, 0.75f, 1f),
        };
        deathsText.AttachNode(this);

        bottomLine = new HorizontalLineNode
        {
            Position = new Vector2(0, TextY + TextHeight + 2),
            Size = new Vector2(560, LineHeight),
            Color = new Vector4(1f, 1f, 1f, 0.25f),
            IsVisible = true,
        };
        bottomLine.AttachNode(this);

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
        if (totalDamageText == null) return;

        topLine.Size = new Vector2(Width, LineHeight);

        float quarter = Width / 4f;
        totalDamageText.Position = new Vector2(8, TextY);
        totalDamageText.Size = new Vector2(quarter - 8, TextHeight);
        raidDpsText.Position = new Vector2(quarter, TextY);
        raidDpsText.Size = new Vector2(quarter, TextHeight);
        durationText.Position = new Vector2(quarter * 2, TextY);
        durationText.Size = new Vector2(quarter, TextHeight);
        deathsText.Position = new Vector2(quarter * 3, TextY);
        deathsText.Size = new Vector2(quarter - 4, TextHeight);

        bottomLine.Position = new Vector2(0, TextY + TextHeight + 2);
        bottomLine.Size = new Vector2(Width, LineHeight);
    }
}
