using KamiToolKit.Components.ConfigurationNodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterConfigurationNode : EntryConfigurationNode<MeterSettings>
{
    private readonly MeterDefinitionConfigurationNode activeNode;

    public MeterConfigurationNode()
    {
        SelectAnItemTextNode.String = "Select a meter or create a new one.";

        activeNode = new MeterDefinitionConfigurationNode();
        activeNode.AttachNode(ConfigurationContentNode);
    }

    protected override void PopulateEntryData(MeterSettings entry)
    {
        activeNode.Size = Size;
        activeNode.SetMeter(entry);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        activeNode.Size = Size;
    }
}
