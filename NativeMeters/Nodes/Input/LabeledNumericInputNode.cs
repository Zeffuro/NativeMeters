using System;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public class LabeledNumericInputNode : LabeledControlRowNode<NumericInputNode>
{
    public LabeledNumericInputNode() : base(new NumericInputNode())
        => MaximumControlWidth = 180.0f;

    public int Value {
        get => ControlNode.Value;
        set => ControlNode.Value = value;
    }

    public int Min {
        get => ControlNode.Min;
        set => ControlNode.Min = value;
    }

    public int Max {
        get => ControlNode.Max;
        set => ControlNode.Max = value;
    }

    public int Step {
        get => ControlNode.Step;
        set => ControlNode.Step = value;
    }

    public bool IsEnabled {
        get => ControlNode.IsEnabled;
        set => ControlNode.IsEnabled = value;
    }

    public Action<int>? OnValueUpdate {
        get => ControlNode.OnValueUpdate;
        set => ControlNode.OnValueUpdate = value;
    }

    public NumericInputNode InnerInput => ControlNode;
}
