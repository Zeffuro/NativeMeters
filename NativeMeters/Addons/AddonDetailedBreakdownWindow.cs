using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Models;
using NativeMeters.Models.Breakdown;
using NativeMeters.Nodes.Breakdown;
using NativeMeters.Services;

namespace NativeMeters.Addons;

public class AddonDetailedBreakdownWindow : NativeAddon
{
    private TabBarNode tabBarNode = null!;
    private EncounterSummaryBarNode summaryBar = null!;
    private ListNode<BreakdownPlayerData, BreakdownPlayerListItemNode> playerListNode = null!;

    // Encounter selector
    private TextButtonNode prevButton = null!;
    private TextButtonNode nextButton = null!;
    private TextNode encounterLabelNode = null!;

    private BreakdownTab currentTab = BreakdownTab.Damage;
    private IMeterService? hookedService;

    private int selectedEncounterIndex = -1; // -1 = live

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        var contentY = ContentStartPosition.Y;
        var contentW = ContentSize.X;
        var contentH = ContentSize.Y;

        prevButton = new TextButtonNode
        {
            Position = ContentStartPosition,
            Size = new Vector2(24, 22),
            String = "◀",
            IsVisible = true,
            OnClick = () => NavigateEncounter(-1),
        };
        prevButton.AttachNode(this);

        encounterLabelNode = new TextNode
        {
            Position = ContentStartPosition with { X = ContentStartPosition.X + 28 },
            Size = new Vector2(contentW - 56, 22),
            FontSize = 13,
            FontType = FontType.Axis,
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Center,
            TextColor = new Vector4(1f, 1f, 1f, 0.9f),
            String = "Live",
            IsVisible = true,
        };
        encounterLabelNode.AttachNode(this);

        nextButton = new TextButtonNode
        {
            Position = ContentStartPosition with { X = ContentStartPosition.X + contentW - 24 },
            Size = new Vector2(24, 22),
            String = "▶",
            IsVisible = true,
            OnClick = () => NavigateEncounter(1),
        };
        nextButton.AttachNode(this);

        summaryBar = new EncounterSummaryBarNode
        {
            Position = ContentStartPosition with { Y = contentY + 24 },
            Size = new Vector2(contentW, 28),
        };
        summaryBar.AttachNode(this);

        tabBarNode = new TabBarNode
        {
            Position = ContentStartPosition with { Y = contentY + 56 },
            Size = new Vector2(contentW, 24),
            IsVisible = true,
        };
        tabBarNode.AttachNode(this);

        tabBarNode.AddTab("Damage", () => SwitchTab(BreakdownTab.Damage));
        tabBarNode.AddTab("Healing", () => SwitchTab(BreakdownTab.Healing));
        tabBarNode.AddTab("Damage Taken", () => SwitchTab(BreakdownTab.DamageTaken));

        var scrollY = contentY + 84;
        var scrollH = Math.Max(0, contentH - 84);

        playerListNode = new ListNode<BreakdownPlayerData, BreakdownPlayerListItemNode>
        {
            Position = ContentStartPosition with { Y = scrollY },
            Size = new Vector2(contentW, scrollH),
            ItemSpacing = 2f,
            OptionsList = [],
        };
        playerListNode.AttachNode(this);

        hookedService = System.InternalMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
        RefreshData();

        base.OnSetup(addon);
    }

    private void NavigateEncounter(int direction)
    {
        var history = System.InternalMeterService.GetEncounterHistory();
        var maxIndex = history.Count - 1;

        if (direction < 0)
        {
            if (selectedEncounterIndex < 0)
                selectedEncounterIndex = 0;
            else if (selectedEncounterIndex < maxIndex)
                selectedEncounterIndex++;
            else
                return;
        }
        else
        {
            if (selectedEncounterIndex > 0)
                selectedEncounterIndex--;
            else if (selectedEncounterIndex == 0)
                selectedEncounterIndex = -1;
            else
                return;
        }

        RefreshData();
    }

    private void UpdateEncounterLabel()
    {
        if (selectedEncounterIndex < 0)
        {
            encounterLabelNode.String = "● Live";
            return;
        }

        var history = System.InternalMeterService.GetEncounterHistory();
        if (selectedEncounterIndex < history.Count)
        {
            var enc = history[selectedEncounterIndex].Encounter;
            var title = enc?.Title ?? "Unknown";
            var dur = enc?.Duration.ToString(@"mm\:ss") ?? "0:00";
            encounterLabelNode.String = $"{title} ({dur})";
        }
        else
        {
            encounterLabelNode.String = "No Data";
        }
    }

    private void SwitchTab(BreakdownTab tab)
    {
        if (currentTab == tab) return;
        currentTab = tab;
        RefreshData();
    }

    private void OnCombatDataUpdated()
    {
        if (selectedEncounterIndex < 0)
            RefreshData();
    }

    private void RefreshData()
    {
        if (playerListNode == null) return;

        UpdateEncounterLabel();

        Encounter? encounter;
        List<Combatant> combatants;

        if (selectedEncounterIndex < 0)
        {
            if (!System.InternalMeterService.HasCombatData())
            {
                playerListNode.OptionsList = [];
                summaryBar.Update(null);
                return;
            }
            encounter = System.InternalMeterService.GetEncounter();
            combatants = System.InternalMeterService.GetCombatants().ToList();
        }
        else
        {
            var history = System.InternalMeterService.GetEncounterHistory();
            if (selectedEncounterIndex >= history.Count)
            {
                playerListNode.OptionsList = [];
                summaryBar.Update(null);
                return;
            }
            var snapshot = history[selectedEncounterIndex];
            encounter = snapshot.Encounter;
            combatants = snapshot.Combatant.Values.ToList();
        }

        summaryBar.Update(encounter);

        combatants.Sort((a, b) => currentTab switch
        {
            BreakdownTab.Damage => b.ENCDPS.CompareTo(a.ENCDPS),
            BreakdownTab.Healing => b.ENCHPS.CompareTo(a.ENCHPS),
            BreakdownTab.DamageTaken => b.Damagetaken.CompareTo(a.Damagetaken),
            _ => b.ENCDPS.CompareTo(a.ENCDPS),
        });

        double duration = encounter?.DURATION ?? 1.0;

        playerListNode.OptionsList = combatants
            .Select(c => new BreakdownPlayerData(c, currentTab, duration))
            .ToList();

        playerListNode.Update();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        if (hookedService != null)
        {
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;
            hookedService = null;
        }

        base.OnFinalize(addon);
    }
}
