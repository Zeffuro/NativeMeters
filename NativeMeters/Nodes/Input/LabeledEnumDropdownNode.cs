using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;
using NativeMeters.Extensions;

namespace NativeMeters.Nodes.Input;

public class LabeledEnumDropdownNode<T> : SimpleComponentNode where T : Enum {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly EnumDropDownNode<T> _dropDownNode;

    public LabeledEnumDropdownNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = string.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _dropDownNode = new EnumDropDownNode<T> {
            Options = new List<T>(),
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

    public Action<T>? OnOptionSelected
    {
        get => _dropDownNode.OnOptionSelected;
        set => _dropDownNode.OnOptionSelected = value;
    }

    public T? SelectedOption
    {
        get => _dropDownNode.OptionListNode.SelectedOption;
        set
        {
            _dropDownNode.OptionListNode.SelectedOption = value;
            if (value != null)
            {
                _dropDownNode.LabelNode.String = value.Description;
            }
        }
    }

    public int MaxListOptions
    {
        get => _dropDownNode.MaxListOptions;
        set => _dropDownNode.MaxListOptions = value;
    }

    public required List<T> Options
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
