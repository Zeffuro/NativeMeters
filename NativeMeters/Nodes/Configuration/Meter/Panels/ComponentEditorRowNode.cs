using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal abstract class ComponentEditorRowNode<TControl> : ResNode, IConfigurationNavigationNode where TControl : NodeBase
{
    private const float MinimumLabelWidth = 72.0f;
    private const float MaximumLabelWidth = 160.0f;
    private const float LabelWidthRatio = 0.42f;

    private readonly LabelTextNode labelNode;

    protected TControl ControlNode { get; }

    protected ComponentEditorRowNode(TControl controlNode)
    {
        labelNode = new LabelTextNode
        {
            String = string.Empty,
            TextFlags = TextFlags.Ellipsis,
        };
        labelNode.AttachNode(this);

        ControlNode = controlNode;
        ControlNode.AttachNode(this);

        Height = 28.0f;
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            labelNode.IsVisible = value;
            ControlNode.IsVisible = value;
            OnVisibilityChanged(value);
        }
    }

    public ReadOnlySeString LabelText
    {
        get => labelNode.String;
        set => labelNode.String = value;
    }

    public TextFlags LabelTextFlags
    {
        get => labelNode.TextFlags;
        set => labelNode.TextFlags = value;
    }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        if (ControlNode is ComponentNode componentNode)
        {
            yield return ConfigurationNavigationTarget.From(componentNode);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        var labelWidth = Math.Clamp(Width * LabelWidthRatio, MinimumLabelWidth, MaximumLabelWidth);
        labelWidth = Math.Min(labelWidth, Width);
        var controlWidth = Math.Max(0.0f, Width - labelWidth);

        labelNode.Position = Vector2.Zero;
        labelNode.Size = new Vector2(labelWidth, Height);

        ControlNode.Position = new Vector2(labelWidth, 0.0f);
        ControlNode.Size = new Vector2(controlWidth, Height);
    }

    protected virtual void OnVisibilityChanged(bool isVisible) { }
}
