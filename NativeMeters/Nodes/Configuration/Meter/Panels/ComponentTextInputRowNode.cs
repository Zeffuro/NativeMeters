using System;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentTextInputRowNode : ComponentEditorRowNode<TextInputNode>
{
    public ComponentTextInputRowNode() : base(new TextInputNode()) { }

    public ReadOnlySeString Text
    {
        get => ControlNode.String;
        set => ControlNode.String = value;
    }

    public string? Placeholder
    {
        get => ControlNode.PlaceholderString;
        set => ControlNode.PlaceholderString = value;
    }

    public new ReadOnlySeString TextTooltip
    {
        get => ControlNode.TextTooltip;
        set => ControlNode.TextTooltip = value;
    }

    public Action<ReadOnlySeString>? OnInputComplete
    {
        get => ControlNode.OnInputComplete;
        set => ControlNode.OnInputComplete = value;
    }

    public TextInputNode InnerInput => ControlNode;
}
