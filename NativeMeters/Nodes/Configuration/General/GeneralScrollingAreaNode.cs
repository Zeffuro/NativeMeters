using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.Dtr;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class GeneralScrollingAreaNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;

    public int TabBarNavIndex { get; set; } = 1;

    public GeneralScrollingAreaNode()
    {
        ReverseContentLayoutUpdate = true;

        GeneralSettings config = System.Config.General;

        new ImportExportResetNode().AttachNode(this);

        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;

        ContentNode.AddNode(
        [new GeneralConfigurationNode(),
            new DtrConfigurationNode(),
            new CheckboxNode {
                Size = new Vector2(300, 20),
                IsVisible = true,
                String = "Debug Mode",
                IsChecked = config.DebugEnabled,
                OnClick = isChecked =>
                {
                    config.DebugEnabled = isChecked;
                }
            }
        ]);

        RecalculateConfigurationLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        RecalculateConfigurationLayout();
    }

    private void RecalculateConfigurationLayout()
    {
        ConfigurationNavigation.Apply(ContentNode, FirstContentNavIndex, TabBarNavIndex, TabBarNavIndex);
    }
}
