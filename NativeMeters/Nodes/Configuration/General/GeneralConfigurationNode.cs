using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.General;

internal sealed class GeneralConfigurationNode : TabbedVerticalListNode
{
    public GeneralConfigurationNode()
    {
        GeneralSettings config = System.Config.General;

        ItemVerticalSpacing = 5;

        var titleNode = new CategoryTextNode
        {
            Height = 18,
            String = "General Configuration",
        };
        AddNode(titleNode);

        AddTab(1);

        var isEnabledCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Enabled",
            IsChecked = config.IsEnabled,
            OnClick = isChecked =>
            {
                config.IsEnabled = isChecked;
            }
        };
        AddNode(isEnabledCheckbox);

        var hideWithNativeUiCheckBox = new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Hide with Native UI",
            IsChecked = config.HideWithNativeUi,
            OnClick = isChecked =>
            {
                config.HideWithNativeUi = isChecked;
            }
        };
        AddNode(hideWithNativeUiCheckBox);

        SubtractTab(1);
    }
}