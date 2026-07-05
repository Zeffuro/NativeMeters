using System;
using System.Collections.Generic;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public class LabeledEnumDropdownNode<T> : LabeledControlRowNode<EnumDropDownNode<T>> where T : struct, Enum
{
    private Action<T>? _onOptionSelected;
    private List<T> _options = [];

    public LabeledEnumDropdownNode() : base(new EnumDropDownNode<T> { Options = [] }) { }

    public Action<T>? OnOptionSelected
    {
        get => _onOptionSelected;
        set
        {
            _onOptionSelected = value;
            ControlNode.OnOptionSelected = value is null
                ? null
                : selected => value(selected);
        }
    }

    public T? SelectedOption
    {
        get => ControlNode.SelectedOption;
        set
        {
            if (value.HasValue)
            {
                ControlNode.SelectedOption = value.Value;
            }
        }
    }

    public int MaxListOptions
    {
        get => ControlNode.MaxListOptions;
        set => ControlNode.MaxListOptions = value;
    }

    public required List<T> Options
    {
        get => _options;
        set
        {
            _options = value;
            ControlNode.Options = value;
        }
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
