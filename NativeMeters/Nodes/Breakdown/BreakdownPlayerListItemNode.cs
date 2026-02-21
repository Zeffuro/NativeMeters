using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownPlayerListItemNode : ListItemNode<BreakdownPlayerData>
{
    private static readonly NumericFormatter Formatter = new();

    private readonly IconImageNode jobIconNode;
    private readonly TextNode nameText;
    private readonly TextNode primaryStatText;
    private readonly TextNode secondaryStatText;

    private readonly BreakdownColumnHeaderNode columnHeader;
    private readonly List<ActionBreakdownRowNode> actionRows = new();

    private bool isExpanded;
    private const float HeaderHeight = 44f;
    private const float ActionRowHeight = 22f;
    private const float ColumnHeaderHeight = 18f;
    private int visibleActionCount;

    public override float ItemHeight
    {
        get
        {
            if (!isExpanded) return HeaderHeight;
            return HeaderHeight + ColumnHeaderHeight + (visibleActionCount * ActionRowHeight);
        }
    }

    public BreakdownPlayerListItemNode()
    {
        jobIconNode = new IconImageNode
        {
            Position = new Vector2(4, 2),
            Size = new Vector2(24, 24),
            FitTexture = true,
        };
        jobIconNode.AttachNode(this);

        nameText = new TextNode
        {
            Position = new Vector2(32, 0),
            Size = new Vector2(400, 22),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(50),
        };
        nameText.AttachNode(this);

        primaryStatText = new TextNode
        {
            Position = new Vector2(32, 22),
            Size = new Vector2(200, 20),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(1f, 1f, 1f, 0.9f),
        };
        primaryStatText.AttachNode(this);

        secondaryStatText = new TextNode
        {
            Position = new Vector2(240, 22),
            Size = new Vector2(260, 20),
            FontSize = 11,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(0.8f, 0.8f, 0.8f, 0.9f),
        };
        secondaryStatText.AttachNode(this);

        columnHeader = new BreakdownColumnHeaderNode
        {
            Position = new Vector2(18, HeaderHeight),
            Size = new Vector2(480, ColumnHeaderHeight),
            IsVisible = false,
        };
        columnHeader.AttachNode(this);

        var headerCollision = new CollisionNode
        {
            Size = new Vector2(500, HeaderHeight),
            ShowClickableCursor = true,
        };
        headerCollision.AddEvent(AtkEventType.MouseClick, ToggleExpand);
        headerCollision.AttachNode(this);

        Size = new Vector2(500, HeaderHeight);
    }

    protected override void SetNodeData(BreakdownPlayerData data)
    {
        if (data == null) return;

        var combatant = data.Combatant;
        var tab = data.Tab;

        nameText.String = combatant.Name;
        nameText.TextColor = combatant.GetColor();

        var iconId = combatant.GetIconId();
        jobIconNode.IsVisible = iconId > 0;
        if (iconId > 0) jobIconNode.IconId = iconId;

        switch (tab)
        {
            case BreakdownTab.Damage:
                var uptimePct = combatant.ActiveTime.HasValue && data.EncounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / data.EncounterDuration * 100.0
                    : 0;
                primaryStatText.String = $"DPS: {Formatter.Format(combatant.ENCDPS, "", "", 0)}   Up: {uptimePct:F0}%";
                secondaryStatText.String = $"Dmg%: {combatant.DamagePercent:F1}%  Crit%: {combatant.CrithitPercent:F1}%  DH%: {combatant.DirectHitPct:F1}%";
                break;

            case BreakdownTab.Healing:
                var healUptimePct = combatant.ActiveTime.HasValue && data.EncounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / data.EncounterDuration * 100.0
                    : 0;
                primaryStatText.String = $"HPS: {Formatter.Format(combatant.ENCHPS, "", "", 0)}   Up: {healUptimePct:F0}%";
                var overHealPct = combatant.Healed > 0 ? combatant.OverHeal * 100.0 / combatant.Healed : 0;
                secondaryStatText.String = $"CritH%: {combatant.CrithealPercent:F1}%  OH: {overHealPct:F1}%";
                break;

            case BreakdownTab.DamageTaken:
                primaryStatText.String = $"Taken: {Formatter.Format(combatant.Damagetaken, "", "m", 1)}";
                secondaryStatText.String = $"Deaths: {combatant.Deaths}  Heals: {Formatter.Format(combatant.Healstaken, "", "m", 1)}";
                break;
        }

        PopulateActions(combatant, tab);
        UpdateExpandedState();
    }

    public override void Update()
    {
        if (ItemData == null) return;
        RepositionHeader();
    }

    public void ToggleExpand()
    {
        isExpanded = !isExpanded;
        UpdateExpandedState();
    }

    private void PopulateActions(Combatant combatant, BreakdownTab tab)
    {
        foreach (var row in actionRows) row.IsVisible = false;
        visibleActionCount = 0;

        if (tab == BreakdownTab.DamageTaken || combatant.ActionBreakdownList == null || combatant.ActionBreakdownList.Count == 0)
            return;

        bool isDamageMode = tab == BreakdownTab.Damage;

        columnHeader.SetTotalLabel(isDamageMode ? "Damage" : "Healing");
        columnHeader.SetPerSecondLabel(isDamageMode ? "DPS" : "HPS");

        var actions = isDamageMode
            ? combatant.ActionBreakdownList.Where(a => a.TotalDamage > 0).OrderByDescending(a => a.TotalDamage).ToList()
            : combatant.ActionBreakdownList.Where(a => a.TotalHealing > 0).OrderByDescending(a => a.TotalHealing).ToList();

        while (actionRows.Count < actions.Count)
        {
            var row = new ActionBreakdownRowNode { IsVisible = false };
            actionRows.Add(row);
            row.AttachNode(this);
        }

        for (int i = 0; i < actions.Count; i++)
        {
            var row = actionRows[i];
            row.IsVisible = isExpanded;
            row.Size = new Vector2(Math.Max(0, Width - 18), ActionRowHeight);
            row.Position = new Vector2(18, HeaderHeight + ColumnHeaderHeight + (i * ActionRowHeight));
            row.SetData(actions[i], isDamageMode);
        }

        visibleActionCount = actions.Count;
    }

    private void UpdateExpandedState()
    {
        bool hasActions = visibleActionCount > 0;
        columnHeader.IsVisible = isExpanded && hasActions;

        for (int i = 0; i < actionRows.Count; i++)
        {
            actionRows[i].IsVisible = isExpanded && i < visibleActionCount;
        }

        for (int i = 0; i < visibleActionCount; i++)
        {
            actionRows[i].Position = new Vector2(18, HeaderHeight + ColumnHeaderHeight + (i * ActionRowHeight));
        }
    }

    private void RepositionHeader()
    {
        if (primaryStatText == null || secondaryStatText == null || columnHeader == null) return;

        nameText.Size = new Vector2(Width - 36, 22);

        primaryStatText.Position = new Vector2(32, 22);
        primaryStatText.Size = new Vector2(200, 20);
        secondaryStatText.Position = new Vector2(240, 22);
        secondaryStatText.Size = new Vector2(Math.Max(0, Width - 244), 20);

        columnHeader.Position = new Vector2(18, HeaderHeight);
        columnHeader.Width = Math.Max(0, Width - 18);

        for (int i = 0; i < visibleActionCount; i++)
        {
            actionRows[i].Width = Math.Max(0, Width - 18);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (primaryStatText == null || secondaryStatText == null || columnHeader == null) return;
        RepositionHeader();
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        if (disposing && actionRows != null)
        {
            foreach (var row in actionRows) row.Dispose();
            actionRows.Clear();
        }
        base.Dispose(disposing, isNativeDestructor);
    }
}
