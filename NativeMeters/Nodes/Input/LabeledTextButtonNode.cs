using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Input;

public unsafe class LabeledTextButtonNode : SimpleComponentNode {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly TextButtonNode _textButtonNode;

    public LabeledTextButtonNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = string.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _textButtonNode = new TextButtonNode{};
        _textButtonNode.AttachNode(_gridNode[1, 0]);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        _gridNode.Size = Size;
        _labelNode.Size = _gridNode[0, 0].Size;
        _textButtonNode.Size = _gridNode[1, 0].Size;
    }

    public ReadOnlySeString ButtonText {
        get => _textButtonNode.String;
        set => _textButtonNode.String = value;
    }

    public ReadOnlySeString LabelText {
        get => _labelNode.String;
        set => _labelNode.String = value;
    }

    public Vector4 LabelTextColor
    {
        get => _labelNode.TextColor;
        set => _labelNode.TextColor = value;
    }

    public TextFlags LabelTextFlags {
        get => _labelNode.TextFlags;
        set => _labelNode.TextFlags = value;
    }

    public Action? OnClick {
        get => _textButtonNode.OnClick;
        set => _textButtonNode.OnClick = value;
    }
}
