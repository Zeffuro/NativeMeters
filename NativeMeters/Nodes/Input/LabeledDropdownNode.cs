using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Input;

public class LabeledDropdownNode : SimpleComponentNode {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly TextDropDownNode _dropDownNode;

    public LabeledDropdownNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = String.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _dropDownNode = new TextDropDownNode {
            Options = new List<string>(),
        };
        _dropDownNode.AttachNode(_gridNode[1, 0]);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        _gridNode.Size = Size;

        _labelNode.Size = _gridNode[0, 0].Size;
        _dropDownNode.Size = _gridNode[1, 0].Size;
    }

    public required ReadOnlySeString LabelText
    {
        get => _labelNode.String;
        set => _labelNode.String = value;
    }

    public Action<string>? OnOptionSelected
    {
        get => _dropDownNode.OnOptionSelected;
        set => _dropDownNode.OnOptionSelected = value;
    }

    public string? SelectedOption
    {
        get => _dropDownNode.SelectedOption;
        set => _dropDownNode.SelectedOption = value;
    }

    public required List<string> Options
    {
        get => _dropDownNode.Options!;
        set => _dropDownNode.Options = value;
    }

    public TextFlags LabelTextFlags
    {
        get => _labelNode.TextFlags;
        set => _labelNode.TextFlags = value;
    }
}
