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
    private ScrollingListNode scrollingContent = null!;
    private BreakdownTableHeaderNode tableHeader = null!;

    private TextButtonNode prevButton = null!;
    private TextButtonNode nextButton = null!;
    private TextNode encounterLabelNode = null!;

    private BreakdownTab currentTab = BreakdownTab.Damage;
    private IMeterService? hookedService;
    private int selectedEncounterIndex = -1;

    private readonly List<BreakdownPlayerSectionNode> playerSections = new();
    private readonly BreakdownTableLayout tableLayout = new();

    private int lastCombatantCount;

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        var contentY = ContentStartPosition.Y;
        var contentW = ContentSize.X;
        var contentH = ContentSize.Y;

        prevButton = new TextButtonNode
        {
            Position = ContentStartPosition,
            Size = new Vector2(70, 22),
            String = "Previous",
            IsVisible = true,
            OnClick = () => NavigateEncounter(-1),
        };
        prevButton.AttachNode(this);

        encounterLabelNode = new TextNode
        {
            Position = ContentStartPosition with { X = ContentStartPosition.X + 74 },
            Size = new Vector2(contentW - 148, 22),
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
            Position = ContentStartPosition with { X = ContentStartPosition.X + contentW - 70 },
            Size = new Vector2(70, 22),
            String = "Next",
            IsVisible = true,
            OnClick = () => NavigateEncounter(1),
        };
        nextButton.AttachNode(this);

        tabBarNode = new TabBarNode
        {
            Position = ContentStartPosition with { Y = contentY + 26 },
            Size = new Vector2(contentW, 24),
            IsVisible = true,
        };
        tabBarNode.AttachNode(this);

        tabBarNode.AddTab("Damage", () => SwitchTab(BreakdownTab.Damage));
        tabBarNode.AddTab("Healing", () => SwitchTab(BreakdownTab.Healing));
        tabBarNode.AddTab("Damage Taken", () => SwitchTab(BreakdownTab.DamageTaken));

        summaryBar = new EncounterSummaryBarNode
        {
            Position = ContentStartPosition with { Y = contentY + 54 },
            Size = new Vector2(contentW, 28),
        };
        summaryBar.AttachNode(this);

        tableHeader = new BreakdownTableHeaderNode
        {
            Position = ContentStartPosition with { X = ContentStartPosition.X + 10, Y = contentY + 84 },
            Size = new Vector2(contentW - 28, 20),
            IsVisible = true,
        };
        tableHeader.AttachNode(this);

        var scrollY = contentY + 106;
        var scrollH = Math.Max(0, contentH - 106);

        scrollingContent = new ScrollingListNode
        {
            Position = ContentStartPosition with { Y = scrollY },
            Size = new Vector2(contentW, scrollH),
            ItemSpacing = 2f,
            FitContents = true,
            IsVisible = true,
        };
        scrollingContent.AttachNode(this);

        hookedService = System.InternalMeterService;
        hookedService.CombatDataUpdated += OnCombatDataUpdated;
        FullRebuild();

        base.OnSetup(addon);
    }

    private void NavigateEncounter(int direction)
    {
        var history = System.InternalMeterService.GetEncounterHistory();
        var maxIndex = history.Count - 1;

        if (direction < 0)
        {
            if (selectedEncounterIndex < 0) selectedEncounterIndex = 0;
            else if (selectedEncounterIndex < maxIndex) selectedEncounterIndex++;
            else return;
        }
        else
        {
            if (selectedEncounterIndex > 0) selectedEncounterIndex--;
            else if (selectedEncounterIndex == 0) selectedEncounterIndex = -1;
            else return;
        }

        FullRebuild();
    }

    private void UpdateEncounterLabel()
    {
        if (selectedEncounterIndex < 0)
        {
            encounterLabelNode.String = "Live";
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
        FullRebuild();
    }

    private void OnCombatDataUpdated()
    {
        if (selectedEncounterIndex < 0)
            UpdateData();
    }

    private void FullRebuild()
    {
        if (scrollingContent == null) return;

        UpdateEncounterLabel();

        scrollingContent.Clear();
        foreach (var section in playerSections) section.Dispose();
        playerSections.Clear();

        var (encounter, combatants) = GetCurrentData();
        if (encounter == null || combatants == null)
        {
            summaryBar.Update(null, currentTab);
            lastCombatantCount = 0;
            return;
        }

        summaryBar.Update(encounter, currentTab);
        SortCombatants(combatants);

        float listWidth = Math.Max(0, scrollingContent.ContentWidth);
        tableHeader.Width = Math.Max(0, listWidth - 18);
        tableHeader.SetLayout(tableLayout);
        bool isDamageMode = currentTab == BreakdownTab.Damage;
        tableHeader.UpdateLabels(isDamageMode ? "Damage" : "Healing", isDamageMode ? "DPS" : "HPS");

        double duration = encounter.DURATION > 0 ? encounter.DURATION : 1.0;

        foreach (var combatant in combatants)
        {
            var section = new BreakdownPlayerSectionNode
            {
                Width = listWidth,
                IsVisible = true,
            };
            section.SetData(combatant, currentTab, duration, tableLayout);
            section.OnToggle = () => scrollingContent.RecalculateLayout();
            playerSections.Add(section);
            scrollingContent.AddNode(section);
        }

        lastCombatantCount = combatants.Count;
        scrollingContent.RecalculateLayout();
    }

    private void UpdateData()
    {
        if (scrollingContent == null) return;

        UpdateEncounterLabel();

        var (encounter, combatants) = GetCurrentData();
        if (encounter == null || combatants == null)
        {
            summaryBar.Update(null, currentTab);
            return;
        }

        summaryBar.Update(encounter, currentTab);
        SortCombatants(combatants);

        tableHeader.Width = Math.Max(0, scrollingContent.ContentWidth - 18);

        if (combatants.Count != lastCombatantCount)
        {
            FullRebuild();
            return;
        }

        double duration = encounter.DURATION > 0 ? encounter.DURATION : 1.0;

        for (int i = 0; i < combatants.Count && i < playerSections.Count; i++)
        {
            playerSections[i].SetData(combatants[i], currentTab, duration, tableLayout);
        }

        scrollingContent.RecalculateLayout();
    }

    private (Encounter? encounter, List<Combatant>? combatants) GetCurrentData()
    {
        if (selectedEncounterIndex < 0)
        {
            if (!System.InternalMeterService.HasCombatData())
                return (null, null);

            return (System.InternalMeterService.GetEncounter(),
                    System.InternalMeterService.GetCombatants().ToList());
        }

        var history = System.InternalMeterService.GetEncounterHistory();
        if (selectedEncounterIndex >= history.Count)
            return (null, null);

        var snapshot = history[selectedEncounterIndex];
        return (snapshot.Encounter, snapshot.Combatant.Values.ToList());
    }

    private void SortCombatants(List<Combatant> combatants)
    {
        combatants.Sort((a, b) => currentTab switch
        {
            BreakdownTab.Damage => b.ENCDPS.CompareTo(a.ENCDPS),
            BreakdownTab.Healing => b.ENCHPS.CompareTo(a.ENCHPS),
            BreakdownTab.DamageTaken => b.Damagetaken.CompareTo(a.Damagetaken),
            _ => b.ENCDPS.CompareTo(a.ENCDPS),
        });
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        if (hookedService != null)
        {
            hookedService.CombatDataUpdated -= OnCombatDataUpdated;
            hookedService = null;
        }

        foreach (var section in playerSections) section.Dispose();
        playerSections.Clear();

        base.OnFinalize(addon);
    }
}
