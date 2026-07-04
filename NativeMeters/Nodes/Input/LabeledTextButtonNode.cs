using System;
using System.Numerics;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Input;

public class LabeledTextButtonNode : LabeledControlRowNode<TextButtonNode>
{
    public LabeledTextButtonNode() : base(new TextButtonNode())
        => MaximumControlWidth = 180.0f;

    public ReadOnlySeString ButtonText {
        get => ControlNode.String;
        set => ControlNode.String = value;
    }

    public Action? OnClick {
        get => ControlNode.OnClick;
        set => ControlNode.OnClick = value;
    }

    public int NavUp
    {
        get => ControlNode.NavUp;
        set => ControlNode.NavUp = value;
    }

    public int NavDown
    {
        get => ControlNode.NavDown;
        set => ControlNode.NavDown = value;
    }

    public int NavLeft
    {
        get => ControlNode.NavLeft;
        set => ControlNode.NavLeft = value;
    }

    public int NavRight
    {
        get => ControlNode.NavRight;
        set => ControlNode.NavRight = value;
    }

    public int NavIndex
    {
        get => ControlNode.NavIndex;
        set => ControlNode.NavIndex = value;
    }
}
