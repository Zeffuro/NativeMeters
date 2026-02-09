using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.General;

internal sealed class GeneralConfigurationNode : TabbedVerticalListNode
{
    public GeneralConfigurationNode()
    {
        GeneralSettings config = System.Config.General;

        ItemVerticalSpacing = 2;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "General Configuration",
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Enabled",
            IsChecked = config.IsEnabled,
            OnClick = isChecked =>
            {
                config.IsEnabled = isChecked;
                ConfigRepository.Save(System.Config);
                System.OverlayManager.Setup();
            },
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 20 },
            String = "Preview",
            IsChecked = config.PreviewEnabled,
            OnClick = isChecked =>
            {
                config.PreviewEnabled = isChecked;
                System.OverlayManager.UpdateActiveService();
            }
        });
        AddNode(new ResNode { Height = 10 });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Hide with Native UI",
            IsChecked = config.HideWithNativeUi,
            OnClick = isChecked =>
            {
                config.HideWithNativeUi = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Replace YOU with your name",
            IsChecked = config.ReplaceYou,
            OnClick = isChecked =>
            {
                config.ReplaceYou = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Privacy Mode",
            IsChecked = config.PrivacyMode,
            TextTooltip = "Shows player jobs instead of names.",
            OnClick = isChecked =>
            {
                config.PrivacyMode = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Enable Encounter History",
            IsChecked = config.EnableEncounterHistory,
            OnClick = isChecked =>
            {
                config.EnableEncounterHistory = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(2, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Auto-switch to live when new fight starts",
            IsChecked = config.AutoSwitchToLiveEncounter,
            OnClick = isChecked =>
            {
                config.AutoSwitchToLiveEncounter = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(2, new LabeledNumericInputNode()
        {
            Size = new Vector2(300, 28),
            IsVisible = true,
            LabelText = "Max History Size",
            Min = 1,
            Max = 50,
            Value = config.MaxEncounterHistory,
            OnValueUpdate = val =>
            {
                System.Config.General.MaxEncounterHistory = val;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            String = "Clear ACT when clearing Meter",
            IsChecked = config.ClearActWithMeter,
            OnClick = val =>
            {
                config.ClearActWithMeter = val;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            String = "Force ACT to end encounter after combat",
            IsChecked = config.ForceEndEncounter,
            OnClick = val =>
            {
                config.ForceEndEncounter = val;
                ConfigRepository.Save(System.Config);
            },
            TextTooltip = "I highly recommend enabling the \"End ACT encounter after wipe/out of combat\" option under Plugins -> OverlayPlugin -> Event Settings instead of using this."
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new TextButtonNode {
            String = "End Encounter",
            Size = new Vector2(120, 28),
            OnClick = () => System.ActiveMeterService.EndEncounter()
        });

        AddNode(1, new TextButtonNode {
            String = "Clear Meter",
            Size = new Vector2(120, 28),
            OnClick = () => System.ActiveMeterService.ClearMeter()
        });

        SubtractTab(1);
    }
}
