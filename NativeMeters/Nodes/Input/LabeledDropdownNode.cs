using System;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public class LabeledDropdownNode : LabeledDropdownRowNode<string, StringDropDownNode>
{
    public LabeledDropdownNode() : base(new StringDropDownNode { Options = [] }) { }

    public Func<string, string> GetOptionLabel
    {
        init => ControlNode.GetLabelFunction = option => value(option);
    }

    public string? SelectedOption
    {
        get => ControlNode.SelectedOption;
        set => ControlNode.SelectedOption = value;
    }
}
