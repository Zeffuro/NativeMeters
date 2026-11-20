using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentStringDropdownRowNode : LabeledDropdownNode
{
    public ComponentStringDropdownRowNode()
    {
        LabelWidth = 128.0f;
        ControlSpacing = 0.0f;
        MaximumControlWidth = float.PositiveInfinity;
    }
}
