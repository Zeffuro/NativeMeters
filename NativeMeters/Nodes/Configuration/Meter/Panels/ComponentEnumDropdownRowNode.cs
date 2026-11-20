using System;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentEnumDropdownRowNode<T> : LabeledEnumDropdownNode<T> where T : struct, Enum
{
    public ComponentEnumDropdownRowNode()
    {
        LabelWidth = 128.0f;
        ControlSpacing = 0.0f;
        MaximumControlWidth = float.PositiveInfinity;
    }
}
