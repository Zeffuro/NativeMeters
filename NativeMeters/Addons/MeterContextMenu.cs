using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.ContextMenu;
using KamiToolKit.Enums;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Localization;
using NativeMeters.Services;
using NativeMeters.Utilities;
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

        var collapseLabel = meterSettings.IsCollapsed ? "▼ " + Loc.Get("MenuExpand") : "▲ " + Loc.Get("MenuCollapse");
        menu.AddItem(collapseLabel, () =>
        {
            meterSettings.IsCollapsed = !meterSettings.IsCollapsed;
        });

        var lockLabel = CheckedLabel(Loc.Get("MenuLocked"), meterSettings.IsLocked);
        menu.AddItem(lockLabel, () =>
        {
            if (!System.Config.Meters.Contains(meterSettings)) return;
            meterSettings.IsLocked = !meterSettings.IsLocked;
            ConfigRepository.Save(System.Config);
        });

        var clickthroughLabel = CheckedLabel(Loc.Get("MeterClickthrough"), meterSettings.IsClickthrough);
        menu.AddItem(clickthroughLabel, () =>
        {
            meterSettings.IsClickthrough = !meterSettings.IsClickthrough;
            System.OverlayManager.Setup();
        });

        var privacyLabel = CheckedLabel(Loc.Get("MenuPrivacy"), System.Config.General.PrivacyMode);
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
                Name = Loc.Get("MenuPreviousEncounters"),
                OnClick = () => { }
            };

            bool isLive = !System.ActiveMeterService.IsViewingHistory;
            historySubMenu.AddItem(CheckedLabel(Loc.Get("MenuLive"), isLive), () =>
            {
                System.ActiveMeterService.SelectLiveEncounter();
            });

            for (int i = 0; i < history.Count; i++)
            {
                var encounter = history[i];
                var capturedIndex = i;
                bool isSelected = System.ActiveMeterService.IsViewingHistory &&
                                  System.ActiveMeterService.SelectedHistoryIndex == capturedIndex;

                var enc = encounter.Encounter;
                var title = enc?.Title ?? Loc.Get("MenuUnknownEncounter");
                var dur = enc?.Duration.ToString(@"mm\:ss") ?? "0:00";
                var rdps = enc != null ? $"{enc.ENCDPS:N0}" : "0";

                var label = isSelected
                    ? $"✓ {title} ({dur}) - {rdps} rDPS"
                    : $"{title} ({dur}) - {rdps} rDPS";

                historySubMenu.AddItem(label, () =>
                {
                    System.ActiveMeterService.SelectEncounter(capturedIndex);
                });
            }

            menu.AddItem(historySubMenu);
        }

        var sortSubMenu = new ContextMenuSubItem
        {
            Name = Loc.Get("MenuSortBy"),
            OnClick = () => { }
        };

        var sortOptions = StatSelector.GetAvailableStatSelectors();
        foreach (var stat in sortOptions)
        {
            bool isSelected = meterSettings.StatToTrack == stat;
            sortSubMenu.AddItem(CheckedLabel(Loc.StatLabel(stat), isSelected), () =>
            {
                meterSettings.StatToTrack = stat;
            });
        }

        menu.AddItem(sortSubMenu);

        menu.AddItem(Separator);

        menu.AddItem(Loc.Get("MenuCopyClipboard"), ClipboardService.CopyEncounterSummary);

        menu.AddItem(Separator);

        menu.AddItem(Loc.EnumLabel(MeterButtonAction.ResetDummies), EnmityUtilities.ResetAllStrikingDummies);

        menu.AddItem(Loc.Get("MenuEndEncounter"), () => System.MeterService.EndEncounter());
        menu.AddItem(Loc.Get("MenuClearMeter"), () => System.MeterService.ClearMeter());

        if (history.Count > 0)
        {
            menu.AddItem(Loc.Get("MenuClearHistory"), () =>
            {
                System.MeterService.ClearHistory();
            });
        }

        menu.AddItem(Separator);

        menu.AddItem(Loc.EnumLabel(MeterButtonAction.DetailedBreakdown), () => System.AddonDetailedBreakdownWindow.Toggle());

        menu.AddItem(Loc.EnumLabel(MeterButtonAction.Settings), () => System.AddonConfigurationWindow.Toggle());

        menu.Open();
    }

    private static string CheckedLabel(string label, bool selected) => selected ? "✓ " + label : label;
}
