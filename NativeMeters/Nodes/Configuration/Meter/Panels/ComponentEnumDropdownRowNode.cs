using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentEnumDropdownRowNode<T> : ComponentEditorRowNode<EnumDropDownNode> where T : struct, Enum
{
    private List<T> options = [];
    private Action<T>? onOptionSelected;

    public ComponentEnumDropdownRowNode() : base(new EnumDropDownNode()) { }

    public List<T> Options
    {
        get => options;
        set
        {
            options = value;
            ControlNode.Options = value.Cast<Enum>().ToList();
        }
    }

    public T? SelectedOption
    {
        get => ControlNode.SelectedOption is T selected ? selected : null;
        set => ControlNode.SelectedOption = value.HasValue ? value.Value : null;
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
                : selected =>
                {
                    if (selected is T typedSelected)
                    {
                        value(typedSelected);
                    }
                };
        }
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
