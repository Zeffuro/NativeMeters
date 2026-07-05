using System;
using System.Collections.Generic;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public class LabeledDropdownNode : LabeledControlRowNode<StringDropDownNode>
{
    public LabeledDropdownNode() : base(new StringDropDownNode { Options = new List<string>() }) { }

    public Action<string>? OnOptionSelected {
        get => ControlNode.OnOptionSelected;
        set => ControlNode.OnOptionSelected = value;
    }

    public string? SelectedOption {
        get => ControlNode.SelectedOption;
        set => ControlNode.SelectedOption = value;
    }

    public int MaxListOptions
    {
        get => ControlNode.MaxListOptions;
        set => ControlNode.MaxListOptions = value;
    }

    public required List<string> Options
    {
        get => ControlNode.Options!;
        set => ControlNode.Options = value;
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
