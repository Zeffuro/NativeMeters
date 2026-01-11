using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.General;

namespace NativeMeters.Nodes.Configuration.Connection;

public sealed class ConnectionScrollingAreaNode : ScrollingListNode
{
    public ConnectionScrollingAreaNode()
    {
        GeneralSettings config = System.Config.General;

        new ImportExportResetNode().AttachNode(this);

        ItemSpacing = 10;

        AddNode(new FunctionalConfigurationNode());

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