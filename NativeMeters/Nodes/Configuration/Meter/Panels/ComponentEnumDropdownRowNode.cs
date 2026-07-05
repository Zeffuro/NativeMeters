using System;
using System.Collections.Generic;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentEnumDropdownRowNode<T> : ComponentEditorRowNode<EnumDropDownNode<T>> where T : struct, Enum
{
    private List<T> options = [];
    private Action<T>? onOptionSelected;

    public ComponentEnumDropdownRowNode() : base(new EnumDropDownNode<T>()) { }

    public List<T> Options
    {
        get => options;
        set
        {
            options = value;
            ControlNode.Options = value;
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

    public Action<T>? OnOptionSelected
    {
        get => onOptionSelected;
        set
        {
            onOptionSelected = value;
            ControlNode.OnOptionSelected = value is null
                ? null
                : selected => value(selected);
        }
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
