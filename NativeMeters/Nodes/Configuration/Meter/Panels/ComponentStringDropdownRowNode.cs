using System;
using System.Collections.Generic;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentStringDropdownRowNode : ComponentEditorRowNode<StringDropDownNode>
{
    public ComponentStringDropdownRowNode() : base(new StringDropDownNode()) { }

    public List<string> Options
    {
        get => ControlNode.Options;
        set => ControlNode.Options = value;
    }

    public string? SelectedOption
    {
        get => ControlNode.SelectedOption;
        set => ControlNode.SelectedOption = value;
    }

    public int MaxListOptions
    {
        get => ControlNode.MaxListOptions;
        set => ControlNode.MaxListOptions = value;
    }

    public Action<string>? OnOptionSelected
    {
        get => ControlNode.OnOptionSelected;
        set => ControlNode.OnOptionSelected = value;
    }

    protected override void OnVisibilityChanged(bool isVisible)
    {
        if (!isVisible) ControlNode.Collapse(playSoundEffect: false);
    }
}
