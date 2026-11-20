using System;
using KamiToolKit.Nodes;
using NativeMeters.Localization;

namespace NativeMeters.Nodes.Input;

public class LabeledEnumDropdownNode<T> : LabeledDropdownRowNode<T, EnumDropDownNode<T>> where T : struct, Enum
{
    public LabeledEnumDropdownNode() : base(new EnumDropDownNode<T>
    {
        Options = [],
        GetLabelFunction = value => Loc.EnumLabel(value),
    }) { }

    public T? SelectedOption
    {
        get => ControlNode.SelectedOption;
        set
        {
            if (value.HasValue)
                ControlNode.SelectedOption = value.Value;
        }
    }
}
