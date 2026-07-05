using System;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentNumericInputRowNode : ComponentEditorRowNode<NumericInputNode>
{
    public ComponentNumericInputRowNode() : base(new NumericInputNode()) { }

    public int Value
    {
        get => ControlNode.Value;
        set => ControlNode.Value = value;
    }

    public int Min
    {
        get => ControlNode.Min;
        set => ControlNode.Min = value;
    }

    public int Max
    {
        get => ControlNode.Max;
        set => ControlNode.Max = value;
    }

    public int Step
    {
        get => ControlNode.Step;
        set => ControlNode.Step = value;
    }

    public Action<int>? OnValueUpdate
    {
        get => ControlNode.OnValueUpdate;
        set => ControlNode.OnValueUpdate = value;
    }

    public NumericInputNode InnerInput => ControlNode;
}
