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

    private BreakdownTab currentTab = BreakdownTab.Damage;
    private IMeterService? hookedService;

    private int selectedEncounterIndex = -1;

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        var contentY = ContentStartPosition.Y;
        var contentW = ContentSize.X;
        var contentH = ContentSize.Y;

        summaryBar = new EncounterSummaryBarNode
        {
            Position = ContentStartPosition,
            Size = new Vector2(contentW, 28),
        };
        summaryBar.AttachNode(this);

        tabBarNode = new TabBarNode
        {
            Position = ContentStartPosition with { Y = contentY + 32 },
            Size = new Vector2(contentW, 24),
            IsVisible = true,
        };
        tabBarNode.AttachNode(this);

        tabBarNode.AddTab("Damage", () => SwitchTab(BreakdownTab.Damage));
        tabBarNode.AddTab("Healing", () => SwitchTab(BreakdownTab.Healing));
        tabBarNode.AddTab("Damage Taken", () => SwitchTab(BreakdownTab.DamageTaken));

        var scrollY = contentY + 60;
        var scrollH = Math.Max(0, contentH - 60);

        playerListNode = new ListNode<BreakdownPlayerData, BreakdownPlayerListItemNode>
        {
            Position = ContentStartPosition with { Y = scrollY },
            Size = new Vector2(contentW, scrollH),
            ItemSpacing = 2f,
            OptionsList = [],
        };
        playerListNode.AttachNode(this);

        hookedService = System.ActiveMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
        RefreshData();

        base.OnSetup(addon);
    }

    private void SwitchTab(BreakdownTab tab)
    {
        if (currentTab == tab) return;
        currentTab = tab;
        RefreshData();
    }

    private void OnCombatDataUpdated()
    {
        RefreshData();
    }

    private void RefreshData()
    {
        if (playerListNode == null) return;

        Encounter? encounter;
        List<Combatant> combatants;

        if (selectedEncounterIndex < 0)
        {
            if (!System.ActiveMeterService.HasCombatData())
            {
                playerListNode.OptionsList = [];
                summaryBar.Update(null);
                return;
            }
            encounter = System.ActiveMeterService.GetEncounter();
            combatants = System.ActiveMeterService.GetCombatants().ToList();
        }
        else
        {
            var history = System.ActiveMeterService.GetEncounterHistory();
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
