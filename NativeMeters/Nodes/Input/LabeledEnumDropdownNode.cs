using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public class LabeledEnumDropdownNode<T> : LabeledControlRowNode<EnumDropDownNode> where T : struct, Enum
{
    private Action<T>? _onOptionSelected;
    private List<T> _options = [];

    public LabeledEnumDropdownNode() : base(new EnumDropDownNode { Options = [] }) { }

    public Action<T>? OnOptionSelected
    {
        get => _onOptionSelected;
        set
        {
            _onOptionSelected = value;
            ControlNode.OnOptionSelected = value is null
                ? null
                : selected =>
                {
                    if (selected is T typedSelected)
                    {
                        value(typedSelected);
                    }
                };
        }
    }

    public T? SelectedOption
    {
        get => ControlNode.SelectedOption is T selected ? selected : null;
        set
        {
            ControlNode.SelectedOption = value.HasValue ? value.Value : null;
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
            ControlNode.Options = value.Cast<Enum>().ToList();
        }
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
