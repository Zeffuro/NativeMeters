using KamiToolKit.Nodes;
using NativeMeters.Nodes.Configuration.General;

namespace NativeMeters.Nodes.Configuration.PartyList;

public sealed class PartyListScrollingAreaNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;

    public int TabBarNavIndex { get; set; } = 1;

    public PartyListScrollingAreaNode()
    {
        new ImportExportResetNode().AttachNode(this);

        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;
        ContentNode.FitWidth = true;

        ContentNode.AddNode(new PartyListMeterConfigurationNode());

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
