using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Services;
using NativeMeters.Tags.Formatting;

namespace NativeMeters.Nodes.Breakdown;

public sealed class BreakdownPlayerSectionNode : LayoutListNode
{
    public const float RowIndent = 8.0f;
    private const float HeaderHeight = 50.0f;
    private static readonly NumericFormatter Formatter = new();

    public bool IsCollapsed { get; private set; } = true;
    public bool FitWidth { get; set; }
    public Action<bool>? OnToggle { get; set; }
    public string CombatantKey { get; private set; } = string.Empty;

    private readonly SimpleNineGridNode headerBackgroundNode;
    private readonly CollisionNode headerCollisionNode;
    private readonly ImageNode toggleArrowNode;
    private readonly IconImageNode jobIconNode;
    private readonly TextNode headerNameText;
    private readonly TextNode primaryStatText;
    private readonly VerticalListNode bodyNode;

    private readonly List<BreakdownTableRowNode> rowPool = new();
    private int visibleRowCount;
    private BreakdownTableLayout? tableLayout;

    public unsafe BreakdownPlayerSectionNode()
    {
        ItemSpacing = 0.0f;

        headerBackgroundNode = new SimpleNineGridNode
        {
            TexturePath = "ui/uld/ListItemB.tex",
            TextureSize = new Vector2(48.0f, 28.0f),
            TextureCoordinates = new Vector2(0.0f, 24.0f),
            NodeFlags = NodeFlags.Visible | NodeFlags.Enabled,
            TopOffset = 10,
            BottomOffset = 10,
            LeftOffset = 15,
            RightOffset = 15,
        };
        headerBackgroundNode.AttachNode(this);

        headerCollisionNode = new CollisionNode
        {
            NodeFlags = NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.HasCollision | NodeFlags.RespondToMouse,
        };
        headerCollisionNode.AttachNode(this);
        headerCollisionNode.AddEvent(AtkEventType.MouseUp, OnClickEvent);

        toggleArrowNode = new ImageNode
        {
            Position = new Vector2(3, 10),
            PartId = (uint)(IsCollapsed ? 1 : 0)
        };
        toggleArrowNode.AddPart([
            new Part { TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(24.0f, 0.0f), Size = new Vector2(24.0f, 24.0f) },
            new Part { TexturePath = "ui/uld/ListItemB.tex", TextureCoordinates = new Vector2(0.0f, 0.0f), Size = new Vector2(24.0f, 24.0f) },
        ]);
        toggleArrowNode.AttachNode(this);

        headerNameText = new TextNode
        {
            Position = new Vector2(52, 5),
            Size = new Vector2(240, 20),
            FontSize = 14,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(1f, 1f, 1f, 1f),
            IsVisible = true,
        };
        headerNameText.AttachNode(this);

        jobIconNode = new IconImageNode
        {
            Position = new Vector2(24, 10),
            Size = new Vector2(24, 24),
            FitTexture = true,
            IsVisible = true,
        };
        jobIconNode.AttachNode(this);

        primaryStatText = new TextNode
        {
            Position = new Vector2(52, 25),
            Size = new Vector2(240, 20),
            FontSize = 12,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            TextColor = new Vector4(0.8f, 0.8f, 0.8f, 1f),
            IsVisible = true,
        };
        primaryStatText.AttachNode(this);

        bodyNode = new VerticalListNode
        {
            FitContents = true,
            ItemSpacing = 1.0f,
            IsVisible = false,
        };
        AddNode(bodyNode);
    }

    private unsafe void OnClickEvent(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData)
    {
        atkEvent->SetEventIsHandled();
        ToggleCollapse();
    }

    private void ToggleCollapse()
    {
        SetCollapsed(!IsCollapsed);
        OnToggle?.Invoke(!IsCollapsed);
    }

    public void SetCollapsed(bool isCollapsed, bool recalculate = true)
    {
        IsCollapsed = isCollapsed;
        toggleArrowNode.PartId = (uint)(IsCollapsed ? 1 : 0);
        bodyNode.IsVisible = !IsCollapsed;

        if (recalculate)
        {
            RecalculateBreakdownLayout();
        }
    }

    public void InitializeLayout(BreakdownTableLayout layout)
    {
        if (tableLayout == layout) return;
        tableLayout = layout;
    }

    public void SetData(Combatant combatant, BreakdownTab tab, double encounterDuration)
    {
        CombatantKey = GetCombatantKey(combatant);
        headerNameText.String = GetDisplayName(combatant);
        headerNameText.TextColor = combatant.GetColor(ColorMode.Job);

        var iconId = combatant.GetIconId();
        jobIconNode.IsVisible = iconId > 0;
        if (iconId > 0) jobIconNode.IconId = iconId;

        switch (tab)
        {
            case BreakdownTab.Damage:
                var uptimePct = combatant.ActiveTime.HasValue && encounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / encounterDuration * 100.0 : 0;
                var critPct = combatant.Swings > 0 ? combatant.Crithits * 100.0 / combatant.Swings : 0;
                var dhPct = combatant.Hits > 0 ? combatant.DirectHitCount * 100.0 / combatant.Hits : 0;
                primaryStatText.String = $"DPS: {Formatter.Format(combatant.ENCDPS, "", "", 0)}  "
                                         + $"Crit: {critPct:F0}%  DH: {dhPct:F0}%  "
                                         + $"Up: {uptimePct:F0}%";
                break;
            case BreakdownTab.Healing:
                var healUp = combatant.ActiveTime.HasValue && encounterDuration > 0
                    ? combatant.ActiveTime.Value.TotalSeconds / encounterDuration * 100.0 : 0;
                var critHealPct = combatant.Heals > 0 ? combatant.Critheals * 100.0 / combatant.Heals : 0;
                var ohPct = combatant.Healed > 0 ? combatant.OverHeal * 100.0 / combatant.Healed : 0;
                primaryStatText.String = $"HPS: {Formatter.Format(combatant.ENCHPS, "", "", 0)}  "
                                         + $"Crit: {critHealPct:F0}%  OH: {ohPct:F0}%  "
                                         + $"Up: {healUp:F0}%";
                break;
        }

        PopulateActions(combatant, tab);
        bodyNode.IsVisible = !IsCollapsed;
        RecalculateBreakdownLayout();
    }

    public static string GetCombatantKey(Combatant combatant)
        => $"{combatant.Name}|{combatant.Job.RowId}";

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
        foreach (var tableRowNode in rowPool)
        {
            tableRowNode.IsVisible = false;
        }
        visibleRowCount = 0;

        if (tableLayout == null
            || combatant.ActionBreakdownList == null || combatant.ActionBreakdownList.Count == 0)
        {
            return;
        }

        bool isDamageMode = tab == BreakdownTab.Damage;
        float rowWidth = Math.Max(0, Width - RowIndent);

        var actions = isDamageMode
            ? combatant.ActionBreakdownList.Where(a => a.TotalDamage > 0).OrderByDescending(a => a.TotalDamage).ToList()
            : combatant.ActionBreakdownList.Where(a => a.TotalHealing > 0).OrderByDescending(a => a.TotalHealing).ToList();

        long maxValue = actions.Count > 0
            ? (isDamageMode ? actions[0].TotalDamage : actions[0].TotalHealing)
            : 1;

        while (rowPool.Count < actions.Count)
        {
            var row = new BreakdownTableRowNode
            {
                X = RowIndent,
                Size = new Vector2(rowWidth, BreakdownTableRowNode.RowHeight),
                IsVisible = false,
            };
            row.SetLayout(tableLayout);
            rowPool.Add(row);
            bodyNode.AddNode(row);
        }

        for (int i = 0; i < actions.Count; i++)
        {
            var row = rowPool[i];
            row.IsVisible = true;
            row.X = RowIndent;
            row.Width = rowWidth;

            long val = isDamageMode ? actions[i].TotalDamage : actions[i].TotalHealing;
            double barPct = maxValue > 0 ? val * 100.0 / maxValue : 0;
            row.SetData(actions[i], isDamageMode, barPct);
        }

        visibleRowCount = actions.Count;
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        if (headerBackgroundNode != null)
        {
            headerBackgroundNode.Size = new Vector2(Width, HeaderHeight);
        }

        if (headerCollisionNode != null)
        {
            headerCollisionNode.Size = new Vector2(Width, HeaderHeight);
            headerCollisionNode.Position = Vector2.Zero;
        }

        if (primaryStatText == null) return;

        headerNameText.Size = new Vector2(Math.Max(0, Width - 60), 20);
        primaryStatText.Size = new Vector2(Math.Max(0, Width - 60), 20);
        bodyNode.Width = Width;

        float rowWidth = Math.Max(0, Width - RowIndent);
        for (int i = 0; i < visibleRowCount && i < rowPool.Count; i++)
        {
            rowPool[i].X = RowIndent;
            rowPool[i].Width = rowWidth;
        }
    }

    protected override void OnRecalculateLayout()
    {
        if (IsCollapsed)
        {
            bodyNode.IsVisible = false;
            Height = HeaderHeight;
        }
        else
        {
            bodyNode.IsVisible = true;
            bodyNode.Y = HeaderHeight + ItemSpacing;
            if (FitWidth)
            {
                bodyNode.Width = Width;
            }
            Height = HeaderHeight + ItemSpacing + bodyNode.Height;
        }
    }

    protected override void OnRecalculateNavigation()
    {
    }

    private void RecalculateBreakdownLayout()
    {
        bodyNode.RecalculateLayout();
        RecalculateLayout();
    }
}
