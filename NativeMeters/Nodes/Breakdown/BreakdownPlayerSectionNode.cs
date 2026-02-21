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
using NativeMeters.Services;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownPlayerSectionNode : CategoryNode
{
    private static readonly NumericFormatter Formatter = new();

    private readonly IconImageNode jobIconNode;
    private readonly TextNode primaryStatText;

    private readonly List<BreakdownTableRowNode> tableRows = new();
    private BreakdownTableLayout? tableLayout;

    public BreakdownPlayerSectionNode()
    {
        IsCollapsed = true;
        HeaderHeight = 46f;
        FontSize = 14;
        NestingIndent = 0.0f;
        CollapsibleContent.ItemVerticalSpacing = 1.0f;
        LabelNode.X = 52.0f;
        LabelNode.Height = 24f;
        LabelNode.Y = 4f;

        jobIconNode = new IconImageNode
        {
            Position = new Vector2(24, 11),
            Size = new Vector2(24, 24),
            FitTexture = true,
            IsVisible = true,
        };
        jobIconNode.AttachNode(HeaderNode);

        primaryStatText = new TextNode
        {
            Position = new Vector2(52, 22),
            Size = new Vector2(240, 20),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(0.8f, 0.8f, 0.8f, 1f),
            IsVisible = true,
        };
        primaryStatText.AttachNode(HeaderNode);
    }

    public void SetData(Combatant combatant, BreakdownTab tab, double encounterDuration, BreakdownTableLayout layout)
    {
        tableLayout = layout;

        String = GetDisplayName(combatant);
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
                break;
            case BreakdownTab.Healing:
                var healUp = combatant.ActiveTime.HasValue && encounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / encounterDuration * 100.0 : 0;
                primaryStatText.String = $"HPS: {Formatter.Format(combatant.ENCHPS, "", "", 0)}  Uptime: {healUp:F0}%";
                break;
            case BreakdownTab.DamageTaken:
                primaryStatText.String = $"Taken: {Formatter.Format(combatant.Damagetaken, "", "m", 1)}";
                break;
        }

        bool wasExpanded = !IsCollapsed;
        PopulateActions(combatant, tab);
        if (wasExpanded && IsCollapsed) IsCollapsed = false;
    }

    private static string GetDisplayName(Combatant combatant)
    {
        if (combatant.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase))
            return combatant.Name;

        if (System.Config.General.PrivacyMode)
        {
            var jobName = combatant.Job.NameEnglish.ToString();
            if (string.IsNullOrEmpty(jobName)) jobName = combatant.Name;
            return combatant.PrivacyIndex.HasValue ? $"{jobName} {combatant.PrivacyIndex.Value}" : jobName;
        }

        if (System.Config.General.ReplaceYou && combatant.Name.Equals("YOU", StringComparison.OrdinalIgnoreCase))
        {
            return Service.ObjectTable.LocalPlayer?.Name.TextValue ?? "YOU";
        }

        return combatant.Name;
    }

    private void PopulateActions(Combatant combatant, BreakdownTab tab)
    {
        Clear();
        foreach (var row in tableRows) row.Dispose();
        tableRows.Clear();

        if (tableLayout == null || tab == BreakdownTab.DamageTaken
            || combatant.ActionBreakdownList == null || combatant.ActionBreakdownList.Count == 0)
            return;

        bool isDamageMode = tab == BreakdownTab.Damage;
        float rowWidth = Math.Max(0, Width - 18);

        var actions = isDamageMode
            ? combatant.ActionBreakdownList.Where(a => a.TotalDamage > 0).OrderByDescending(a => a.TotalDamage).ToList()
            : combatant.ActionBreakdownList.Where(a => a.TotalHealing > 0).OrderByDescending(a => a.TotalHealing).ToList();

        long maxValue = actions.Count > 0
            ? (isDamageMode ? actions.Max(a => a.TotalDamage) : actions.Max(a => a.TotalHealing))
            : 1;

        foreach (var action in actions)
        {
            var row = new BreakdownTableRowNode
            {
                Size = new Vector2(rowWidth, BreakdownTableRowNode.RowHeight),
                IsVisible = true,
            };
            row.SetLayout(tableLayout);

            long val = isDamageMode ? action.TotalDamage : action.TotalHealing;
            double barPct = maxValue > 0 ? val * 100.0 / maxValue : 0;

            row.SetData(action, isDamageMode, barPct);
            tableRows.Add(row);
            AddNode(row);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (primaryStatText == null) return;

        primaryStatText.Size = new Vector2(Math.Max(0, Width - 60), 20);

        float rowWidth = Math.Max(0, Width - 18);
        foreach (var row in tableRows) row.Width = rowWidth;
    }
}
