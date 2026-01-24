using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
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
                Util.SaveConfig(System.Config);
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

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Hide with Native UI",
            IsChecked = config.HideWithNativeUi,
            OnClick = isChecked =>
            {
                config.HideWithNativeUi = isChecked;
                Util.SaveConfig(System.Config);
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
                Util.SaveConfig(System.Config);
            }
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Clear ACT when clearing Meter",
            IsChecked = config.ClearActWithMeter,
            OnClick = val =>
            {
                config.ClearActWithMeter = val;
                Util.SaveConfig(System.Config);
            }
        });

        AddNode(1, new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Force ACT to end encounter after combat",
            IsChecked = config.ForceEndEncounter,
            OnClick = val =>
            {
                config.ForceEndEncounter = val;
                Util.SaveConfig(System.Config);
            },
            TextTooltip = "I highly recommend enabling the \"End ACT encounter after wipe/out of combat\" option under Plugins -> OverlayPlugin -> Event Settings instead of using this."
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new TextButtonNode {
            String = "End Encounter",
            Size = new Vector2(120, 28),
            OnClick = () => System.MeterService.EndEncounter()
        });

        AddNode(1, new TextButtonNode {
            String = "Clear Meter",
            Size = new Vector2(120, 28),
            OnClick = () => System.MeterService.ClearMeter()
        });

        SubtractTab(1);
    }
}
