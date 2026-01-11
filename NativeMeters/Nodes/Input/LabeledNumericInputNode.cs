using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

public unsafe class LabeledNumericInputNode : SimpleComponentNode {
    private readonly GridNode _gridNode;
    private readonly TextNode _labelNode;
    private readonly NumericInputNode _numericInputNode;

    public LabeledNumericInputNode() {
        _gridNode = new GridNode {
            GridSize = new GridSize(2, 1),
        };
        _gridNode.AttachNode(this);

        _labelNode = new LabelTextNode {
            String = string.Empty,
        };
        _labelNode.AttachNode(_gridNode[0, 0]);

        _numericInputNode = new NumericInputNode();
        _numericInputNode.AttachNode(_gridNode[1, 0]);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        _gridNode.Size = Size;
        _labelNode.Size = _gridNode[0, 0].Size;
        _numericInputNode.Size = _gridNode[1, 0].Size;
    }

    public string LabelText {
        get => _labelNode.String;
        set => _labelNode.String = value;
    }

    public TextFlags LabelTextFlags {
        get => _labelNode.TextFlags;
        set => _labelNode.TextFlags = value;
    }

    public int Value {
        get => _numericInputNode.Value;
        set => _numericInputNode.Value = value;
    }

    public int Min {
        get => _numericInputNode.Min;
        set => _numericInputNode.Min = value;
    }

    public int Max {
        get => _numericInputNode.Max;
        set => _numericInputNode.Max = value;
    }

    public int Step {
        get => _numericInputNode.Step;
        set => _numericInputNode.Step = value;
    }

    public Action<int>? OnValueUpdate {
        get => _numericInputNode.OnValueUpdate;
        set => _numericInputNode.OnValueUpdate = value;
    }

    public NumericInputNode InnerInput => _numericInputNode;
}