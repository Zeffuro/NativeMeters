using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;

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

        SubtractTab(1);
    }
}
