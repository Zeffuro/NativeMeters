using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownPlayerSectionNode : CategoryNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly IconImageNode jobIconNode;
    private readonly TextNode primaryStatText;
    private readonly TextNode secondaryStatText;

    private BreakdownTableHeaderNode? tableHeader;
    private readonly List<BreakdownTableRowNode> tableRows = new();
    private BreakdownTableLayout? tableLayout;

    public BreakdownPlayerSectionNode()
    {
        IsCollapsed = true;
        HeaderHeight = 44f;
        FontSize = 14;
        NestingIndent = 0.0f;

        LabelNode.X = 52.0f;
        LabelNode.Height = 22f;
        LabelNode.Y = 0f;

        jobIconNode = new IconImageNode
        {
            Position = new Vector2(28, 2),
            Size = new Vector2(20, 20),
            FitTexture = true,
            IsVisible = true,
        };
        jobIconNode.AttachNode(HeaderNode);

        primaryStatText = new TextNode
        {
            Position = new Vector2(28, 22),
            Size = new Vector2(220, 20),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(0.85f, 0.85f, 0.85f, 1f),
            IsVisible = true,
        };
        primaryStatText.AttachNode(HeaderNode);

        secondaryStatText = new TextNode
        {
            Position = new Vector2(260, 22),
            Size = new Vector2(260, 20),
            FontSize = 11,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Right,
            TextColor = new Vector4(0.65f, 0.65f, 0.65f, 1f),
            IsVisible = true,
        };
        secondaryStatText.AttachNode(HeaderNode);
    }

    public void SetData(Combatant combatant, BreakdownTab tab, double encounterDuration, BreakdownTableLayout layout)
    {
        tableLayout = layout;

        String = combatant.Name;
        LabelNode.TextColor = combatant.GetColor();

        var iconId = combatant.GetIconId();
        jobIconNode.IsVisible = iconId > 0;
        if (iconId > 0) jobIconNode.IconId = iconId;

        switch (tab)
        {
            case BreakdownTab.Damage:
                var uptimePct = combatant.ActiveTime.HasValue && encounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / encounterDuration * 100.0 : 0;
                primaryStatText.String = $"DPS: {Formatter.Format(combatant.ENCDPS, "", "", 0)}  Uptime: {uptimePct:F0}%";
                secondaryStatText.String = $"Dmg: {combatant.DamagePercent:F1}%  C: {combatant.CrithitPercent:F1}%  DH: {combatant.DirectHitPct:F1}%";
                break;
            case BreakdownTab.Healing:
                var healUp = combatant.ActiveTime.HasValue && encounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / encounterDuration * 100.0 : 0;
                primaryStatText.String = $"HPS: {Formatter.Format(combatant.ENCHPS, "", "", 0)}  Uptime: {healUp:F0}%";
                var oh = combatant.Healed > 0 ? combatant.OverHeal * 100.0 / combatant.Healed : 0;
                secondaryStatText.String = $"CritH: {combatant.CrithealPercent:F1}%  OH: {oh:F1}%";
                break;
            case BreakdownTab.DamageTaken:
                primaryStatText.String = $"Taken: {Formatter.Format(combatant.Damagetaken, "", "m", 1)}";
                secondaryStatText.String = $"Deaths: {combatant.Deaths}  Heals: {Formatter.Format(combatant.Healstaken, "", "m", 1)}";
                break;
        }

        bool wasExpanded = !IsCollapsed;
        PopulateActions(combatant, tab);
        if (wasExpanded && IsCollapsed) IsCollapsed = false;
    }

    private void PopulateActions(Combatant combatant, BreakdownTab tab)
    {
        Clear();
        foreach (var row in tableRows) row.Dispose();
        tableRows.Clear();
        tableHeader?.Dispose();
        tableHeader = null;

        if (tableLayout == null || tab == BreakdownTab.DamageTaken
            || combatant.ActionBreakdownList == null || combatant.ActionBreakdownList.Count == 0)
            return;

        bool isDamageMode = tab == BreakdownTab.Damage;
        float rowWidth = Math.Max(0, Width - 18);

        tableHeader = new BreakdownTableHeaderNode
        {
            Width = rowWidth,
            IsVisible = true,
        };
        tableHeader.SetLayout(tableLayout);
        tableHeader.UpdateLabels(isDamageMode ? "Damage" : "Healing", isDamageMode ? "DPS" : "HPS");
        AddNode(tableHeader);

        var actions = isDamageMode
            ? combatant.ActionBreakdownList.Where(a => a.TotalDamage > 0).OrderByDescending(a => a.TotalDamage).ToList()
            : combatant.ActionBreakdownList.Where(a => a.TotalHealing > 0).OrderByDescending(a => a.TotalHealing).ToList();

        foreach (var action in actions)
        {
            var row = new BreakdownTableRowNode
            {
                Size = new Vector2(rowWidth, 22),
                IsVisible = true,
            };
            row.SetLayout(tableLayout);
            row.SetData(action, isDamageMode);
            tableRows.Add(row);
            AddNode(row);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (primaryStatText == null || secondaryStatText == null) return;

        secondaryStatText.Position = new Vector2(Math.Max(260, Width - 280), 22);
        secondaryStatText.Size = new Vector2(Math.Max(0, Width - secondaryStatText.X - 4), 20);

        float rowWidth = Math.Max(0, Width - 18);
        if (tableHeader != null) tableHeader.Width = rowWidth;
        foreach (var row in tableRows) row.Width = rowWidth;
    }
}
