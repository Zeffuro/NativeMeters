using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using Lumina.Text.ReadOnly;
using NativeMeters.Nodes.Configuration;

namespace NativeMeters.Nodes.Input;

public class LabeledTextButtonNode : ResNode, IConfigurationNavigationNode {
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

    public override bool IsVisible {
        get => base.IsVisible;
        set {
            base.IsVisible = value;
            _gridNode.IsVisible = value;
            _labelNode.IsVisible = value;
            _textButtonNode.IsVisible = value;
        }
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

    public int NavUp
    {
        get => _textButtonNode.NavUp;
        set => _textButtonNode.NavUp = value;
    }

    public int NavDown
    {
        get => _textButtonNode.NavDown;
        set => _textButtonNode.NavDown = value;
    }

    public int NavLeft
    {
        get => _textButtonNode.NavLeft;
        set => _textButtonNode.NavLeft = value;
    }

    public int NavRight
    {
        get => _textButtonNode.NavRight;
        set => _textButtonNode.NavRight = value;
    }

    public int NavIndex
    {
        get => _textButtonNode.NavIndex;
        set => _textButtonNode.NavIndex = value;
    }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        yield return ConfigurationNavigationTarget.From(_textButtonNode);
    }
}
