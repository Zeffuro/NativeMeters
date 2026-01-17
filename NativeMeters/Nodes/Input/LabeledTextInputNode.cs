using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Input;

public class LabeledTextInputNode : SimpleComponentNode {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly TextInputNode _textInputNode;

    public LabeledTextInputNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = string.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _textInputNode = new TextInputNode();
        _textInputNode.AttachNode(_gridNode[1, 0]);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        _gridNode.Size = Size;
        _labelNode.Size = _gridNode[0, 0].Size;
        _textInputNode.Size = _gridNode[1, 0].Size;
    }

    public ReadOnlySeString LabelText {
        get => _labelNode.String;
        set => _labelNode.String = value;
    }

    public ReadOnlySeString Text {
        get => _textInputNode.String;
        set => _textInputNode.String = value;
    }

    public string? Placeholder {
        get => _textInputNode.PlaceholderString;
        set => _textInputNode.PlaceholderString = value;
    }

    public TextFlags LabelTextFlags {
        get => _labelNode.TextFlags;
        set => _labelNode.TextFlags = value;
    }

    public Action<ReadOnlySeString>? OnInputReceived {
        get => _textInputNode.OnInputReceived;
        set => _textInputNode.OnInputReceived = value;
    }

    public Action<ReadOnlySeString>? OnInputComplete {
        get => _textInputNode.OnInputComplete;
        set => _textInputNode.OnInputComplete = value;
    }

    public TextInputNode InnerInput => _textInputNode;
}