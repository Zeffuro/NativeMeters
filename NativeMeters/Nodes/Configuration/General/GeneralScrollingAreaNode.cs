using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Addons;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.Dtr;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class GeneralScrollingAreaNode : ScrollingListNode
{
    public GeneralScrollingAreaNode()
    {
        GeneralSettings config = System.Config.General;

        new ImportExportResetNode().AttachNode(this);

        ItemSpacing = 10;

        AddNode(new GeneralConfigurationNode());

        AddNode(new DtrConfigurationNode());

        AddNode(new CheckboxNode
        {
            Size = new Vector2(300, 20),
            IsVisible = true,
            String = "Debug Mode",
            IsChecked = config.DebugEnabled,
            OnClick = isChecked =>
            {
                config.DebugEnabled = isChecked;
            }
        });
    }
}
