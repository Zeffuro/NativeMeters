using KamiToolKit.Premade.Nodes;
using NativeMeters.Addons;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterConfigurationNode : ConfigNode<MeterWrapper>
{
    private MeterDefinitionConfigurationNode? activeNode;

    protected override void OptionChanged(MeterWrapper? option)
    {
        if (option == null)
        {
            if (activeNode != null) activeNode.IsVisible = false;
            return;
        }

        if (activeNode == null)
        {
            activeNode = new MeterDefinitionConfigurationNode();
            activeNode.AttachNode(this);
        }

        activeNode.IsVisible = true;
        activeNode.Size = Size;
        activeNode.SetMeter(option.MeterSettings);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (activeNode != null) activeNode.Size = Size;
    }
}