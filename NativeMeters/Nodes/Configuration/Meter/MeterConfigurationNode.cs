using System;
using KamiToolKit.Components.ConfigurationNodes;
using NativeMeters.Configuration;
using NativeMeters.Localization;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterConfigurationNode : EntryConfigurationNode<MeterSettings>
{
    private readonly MeterDefinitionConfigurationNode activeNode;

    internal MeterConfigurationNode(MeterImportConfirmationAddon importConfirmation, Action<MeterSettings>? onNameChanged = null)
    {
        SelectAnItemTextNode.String = Loc.Get("MeterSelectOrCreate");

        activeNode = new MeterDefinitionConfigurationNode(importConfirmation, onNameChanged);
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
