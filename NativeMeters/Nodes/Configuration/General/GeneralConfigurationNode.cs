using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.General;

internal sealed class GeneralConfigurationNode : TabbedVerticalListNode
{
    private const float CheckboxHeight = 20.0f;

    public GeneralConfigurationNode(Action<string?> changeLanguage)
    {
        GeneralSettings config = System.Config.General;

        ItemSpacing = 2;

        AddNode(1, [
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                IsVisible = true,
                String = Loc.Get("GeneralEnabled"),
                IsChecked = config.IsEnabled,
                OnClick = isChecked =>
                {
                    config.IsEnabled = isChecked;
                    ConfigRepository.Save(System.Config);
                    System.OverlayManager.Setup();
                    System.PartyListMeterManager?.UpdateSettings();
                },
            },
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                String = Loc.Get("GeneralPreview"),
                IsChecked = config.PreviewEnabled,
                OnClick = isChecked =>
                {
                    config.PreviewEnabled = isChecked;
                    System.OverlayManager.UpdateActiveService();
                    System.PartyListMeterManager?.UpdateSettings();
                }
            },
            LanguageDropdown.Create(changeLanguage),
        ]);

        AddNode(new ResNode { Height = 10 });

        AddNode(1, [
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                IsVisible = true,
                String = Loc.Get("GeneralReplaceYou"),
                IsChecked = config.ReplaceYou,
                OnClick = isChecked =>
                {
                    config.ReplaceYou = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                IsVisible = true,
                String = Loc.Get("GeneralPrivacy"),
                IsChecked = config.PrivacyMode,
                TextTooltip = Loc.Get("GeneralPrivacyHelp"),
                OnClick = isChecked =>
                {
                    config.PrivacyMode = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        AddNode(new ResNode { Height = 10 });

        AddNode(1, [
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                String = Loc.Get("GeneralBreakdownParser"),
                IsChecked = config.EnableInternalParserForBreakdown,
                OnClick = isChecked =>
                {
                    config.EnableInternalParserForBreakdown = isChecked;
                    ConfigRepository.Save(System.Config);

                    if (isChecked)
                    {
                        if (System.InternalMeterService.IsDisposed)
                        {
                            System.InternalMeterService = new Services.Internal.InternalMeterService();
                            System.AddonDetailedBreakdownWindow.ReSubscribeToEvents();
                        }

                        System.InternalMeterService.Enable();
                    }
                    else if (!System.Config.ConnectionSettings.SelectedConnectionType.Equals(ConnectionType.Internal))
                    {
                        System.InternalMeterService.Dispose();
                    }
                }
            },
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                IsVisible = true,
                String = Loc.Get("GeneralHistoryEnabled"),
                IsChecked = config.EnableEncounterHistory,
                OnClick = isChecked =>
                {
                    config.EnableEncounterHistory = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        AddNode(2, [
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                IsVisible = true,
                String = Loc.Get("GeneralSwitchLive"),
                IsChecked = config.AutoSwitchToLiveEncounter,
                OnClick = isChecked =>
                {
                    config.AutoSwitchToLiveEncounter = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new LabeledNumericInputNode()
            {
                Size = new Vector2(300, 28),
                IsVisible = true,
                LabelText = Loc.Get("GeneralHistorySize"),
                Min = 1,
                Max = 50,
                Value = config.MaxEncounterHistory,
                OnValueUpdate = val =>
                {
                    System.Config.General.MaxEncounterHistory = val;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        AddNode(new ResNode { Height = 10 });

        AddNode(1, [
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                String = Loc.Get("GeneralClearAct"),
                IsChecked = config.ClearActWithMeter,
                OnClick = val =>
                {
                    config.ClearActWithMeter = val;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxRowNode
            {
                Height = CheckboxHeight,
                String = Loc.Get("GeneralEndAct"),
                IsChecked = config.ForceEndEncounter,
                OnClick = val =>
                {
                    config.ForceEndEncounter = val;
                    ConfigRepository.Save(System.Config);
                },
                TextTooltip = Loc.Get("GeneralEndActHelp")
            },
        ]);

        AddNode(new ResNode { Height = 10 });

        var encounterButtonRow = new HorizontalListNode
        {
            Height = 28,
            ItemSpacing = 8,
        };
        encounterButtonRow.AddNode([
            new TextButtonNode {
                String = Loc.Get("GeneralEndEncounter"),
                Size = new Vector2(120, 28),
                OnClick = () => System.ActiveMeterService.EndEncounter()
            },
            new TextButtonNode {
                String = Loc.Get("GeneralClearMeter"),
                Size = new Vector2(120, 28),
                OnClick = () => System.ActiveMeterService.ClearMeter()
            },
        ]);
        AddNode(1, encounterButtonRow);

        RecalculateLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        FitWidth = Width >= 2 * TabSize + ItemSpacing;
    }
}
