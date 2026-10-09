using System;
using KamiToolKit.Components.ConfigurationNodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterConfigurationNode : EntryConfigurationNode<MeterSettings>
{
    private readonly MeterDefinitionConfigurationNode activeNode;

    public MeterConfigurationNode(Action<MeterSettings>? onNameChanged = null)
    {
        SelectAnItemTextNode.String = "Select a meter or create a new one.";

        activeNode = new MeterDefinitionConfigurationNode(onNameChanged);
        activeNode.AttachNode(ConfigurationContentNode);
    }

    protected override void PopulateEntryData(MeterSettings entry)
    {
        ConfigurationLayout.SetSize(activeNode, Size);
        activeNode.SetMeter(entry);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ConfigurationLayout.SetSize(activeNode, Size);
    }
}
