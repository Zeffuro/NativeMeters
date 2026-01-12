using KamiToolKit.Premade.Nodes;
using NativeMeters.Addons;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterConfigurationNode : ConfigNode<MeterWrapper>
{
    private MeterDefinitionConfigurationNode? _activeNode;

    protected override void OptionChanged(MeterWrapper? option)
    {
        if (option == null)
        {
            if (_activeNode != null) _activeNode.IsVisible = false;
            return;
        }

        if (_activeNode == null)
        {
            _activeNode = new MeterDefinitionConfigurationNode();
            _activeNode.AttachNode(this);
        }

        _activeNode.IsVisible = true;
        _activeNode.Size = Size;
        _activeNode.SetMeter(option.MeterSettings);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_activeNode != null) _activeNode.Size = Size;
    }
}