using KamiToolKit.BaseTypes;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal abstract class ComponentEditorRowNode<TControl> : LabeledControlRowNode<TControl> where TControl : NodeBase
{
    protected ComponentEditorRowNode(TControl controlNode) : base(controlNode)
    {
        LabelWidth = 128.0f;
        ControlSpacing = 0.0f;
        MaximumControlWidth = float.PositiveInfinity;
    }
}
