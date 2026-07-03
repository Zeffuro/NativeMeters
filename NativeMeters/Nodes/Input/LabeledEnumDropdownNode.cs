using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using Lumina.Text.ReadOnly;
using NativeMeters.Nodes.Configuration;

namespace NativeMeters.Nodes.Input;

public class LabeledEnumDropdownNode<T> : SimpleComponentNode, IConfigurationNavigationNode where T : struct, Enum {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly EnumDropDownNode _dropDownNode;
    private Action<T>? _onOptionSelected;
    private List<T> _options = [];

    public LabeledEnumDropdownNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = string.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _dropDownNode = new EnumDropDownNode {
            Options = [],
        };
        _dropDownNode.AttachNode(_gridNode[1, 0]);

        FocusNode = _dropDownNode.CollisionNode;
    }

    public override bool IsVisible {
        get => base.IsVisible;
        set {
            base.IsVisible = value;
            _gridNode.IsVisible = value;
            _labelNode.IsVisible = value;
            _dropDownNode.IsVisible = value;
        }
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
        get => _onOptionSelected;
        set
        {
            _onOptionSelected = value;
            _dropDownNode.OnOptionSelected = value is null
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
        get => _dropDownNode.SelectedOption is T selected ? selected : null;
        set
        {
            _dropDownNode.SelectedOption = value.HasValue ? value.Value : null;
        }
    }

    public int MaxListOptions
    {
        get => _dropDownNode.MaxListOptions;
        set => _dropDownNode.MaxListOptions = value;
    }

    public required List<T> Options
    {
        get => _options;
        set
        {
            _options = value;
            _dropDownNode.Options = value.Cast<Enum>().ToList();
        }
    }

    public TextFlags LabelTextFlags
    {
        get => _labelNode.TextFlags;
        set => _labelNode.TextFlags = value;
    }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        yield return ConfigurationNavigationTarget.From(_dropDownNode);
    }
}
