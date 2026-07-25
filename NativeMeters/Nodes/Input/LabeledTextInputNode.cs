using System;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Input;

public class LabeledTextInputNode : LabeledControlRowNode<TextInputNode>
{
    public LabeledTextInputNode() : base(new TextInputNode()) { }

    public ReadOnlySeString Text {
        get => ControlNode.String;
        set => ControlNode.String = value;
    }

    public string? Placeholder {
        get => ControlNode.PlaceholderString;
        set => ControlNode.PlaceholderString = value;
    }

    public Action<ReadOnlySeString>? OnInputReceived {
        get => ControlNode.OnInputReceived;
        set => ControlNode.OnInputReceived = value;
    }

    public Action<ReadOnlySeString>? OnInputComplete {
        get => ControlNode.OnInputComplete;
        set => ControlNode.OnInputComplete = value;
    }

    public bool IsEnabled {
        get => ControlNode.IsEnabled;
        set
        {
            ControlNode.IsEnabled = value;
            Alpha = value ? 1.0f : 0.45f;
        }
    }

    public TextInputNode InnerInput => ControlNode;
}
