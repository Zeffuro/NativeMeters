using System.ComponentModel;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.ContextMenu;
using KamiToolKit.Enums;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Services;
using ContextMenu = KamiToolKit.ContextMenu.ContextMenu;

namespace NativeMeters.Addons;

public static class MeterContextMenu
{
    private static ContextMenuItem Separator => new()
    {
        Name = "---------------------------",
        IsEnabled = false,
        OnClick = () => { }
    };

    // Temporary method, since OwnerAddon is never set for Overlays we need to set this or else the contextmenu will close immediately
    // TODO: Figure out how to do this in the ContextMenu itself or have Kami come up with a solution
    // The better solution is to use agentContextMenu->OpenContextMenu(false, true); in KTK when it's an overlay so it doesn't bind to the addon
    private static unsafe void SetAgentOwnerAddon()
    {
        var agentContext = AgentContext.Instance();
        agentContext->OwnerAddon = RaptureAtkUnitManager.Instance()->GetAddonByName(OverlayLayer.BehindUserInterface.Description)->Id;
    }

    public static void Open(MeterSettings meterSettings, ContextMenu menu)
    {
        if (menu == null || meterSettings == null) return;

        SetAgentOwnerAddon();

        menu.Clear();

        string collapseLabel = meterSettings.IsCollapsed ? "▼ Expand" : "▲ Collapse";
        menu.AddItem(collapseLabel, () =>
        {
            meterSettings.IsCollapsed = !meterSettings.IsCollapsed;
        });

        string lockLabel = meterSettings.IsLocked ? "✓ Locked" : "Locked";
        menu.AddItem(lockLabel, () =>
        {
            meterSettings.IsLocked = !meterSettings.IsLocked;
        });

        string clickthroughLabel = meterSettings.IsClickthrough ? "✓ Clickthrough" : "Clickthrough";
        menu.AddItem(clickthroughLabel, () =>
        {
            meterSettings.IsClickthrough = !meterSettings.IsClickthrough;
            System.OverlayManager.Setup();
        });

        string privacyLabel = System.Config.General.PrivacyMode ? "✓ Privacy Mode" : "Privacy Mode";
        menu.AddItem(privacyLabel, () =>
        {
            System.Config.General.PrivacyMode = !System.Config.General.PrivacyMode;
        });

        var history = System.ActiveMeterService.GetEncounterHistory();
        if (history.Count > 0)
        {
            menu.AddItem(Separator);

            var historySubMenu = new ContextMenuSubItem
            {
                Name = "Previous Encounters...",
                OnClick = () => { }
            };

            bool isLive = !System.ActiveMeterService.IsViewingHistory;
            historySubMenu.AddItem(isLive ? "✓ Live (Current)" : "Live (Current)", () =>
            {
                System.ActiveMeterService.SelectLiveEncounter();
            });

            for (int i = 0; i < history.Count; i++)
            {
                var encounter = history[i];
                var capturedIndex = i;
                bool isSelected = System.ActiveMeterService.IsViewingHistory &&
                                  System.ActiveMeterService.SelectedHistoryIndex == capturedIndex;

                string name = encounter.Encounter?.Title ?? "Unknown";
                string duration = encounter.Encounter?.Duration.ToString(@"mm\:ss") ?? "??:??";
                string label = isSelected ? $"✓ {name} ({duration})" : $"  {name} ({duration})";

                historySubMenu.AddItem(label, () =>
                {
                    System.ActiveMeterService.SelectEncounter(capturedIndex);
                });
            }

            menu.AddItem(historySubMenu);
        }

        var sortSubMenu = new ContextMenuSubItem
        {
            Name = "Sort By...",
            OnClick = () => { }
        };

        var sortOptions = StatSelector.GetAvailableStatSelectors();
        foreach (var stat in sortOptions)
        {
            bool isSelected = meterSettings.StatToTrack == stat;
            sortSubMenu.AddItem(isSelected ? $"✓ {stat}" : $"  {stat}", () =>
            {
                meterSettings.StatToTrack = stat;
            });
        }

        menu.AddItem(sortSubMenu);

        menu.AddItem(Separator);

        menu.AddItem("Copy to Clipboard", ClipboardService.CopyEncounterSummary);

        menu.AddItem(Separator);

        menu.AddItem("End Encounter", () => System.MeterService.EndEncounter());
        menu.AddItem("Clear Meter", () => System.MeterService.ClearMeter());

        if (history.Count > 0)
        {
            menu.AddItem("Clear History", () =>
            {
                System.MeterService.ClearHistory();
            });
        }

        menu.AddItem(Separator);

        menu.AddItem("Detailed Breakdown", () => System.AddonDetailedBreakdownWindow.Toggle());

        menu.AddItem("Settings", () => System.AddonConfigurationWindow.Toggle());

        menu.Open();
    }
}
