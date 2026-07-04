using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;
using NativeMeters.Nodes.Configuration;

namespace NativeMeters.Nodes.Input;

public abstract class LabeledControlRowNode<TControl> : ResNode, IConfigurationNavigationNode where TControl : NodeBase
{
    protected readonly LabelTextNode LabelNode;

    protected TControl ControlNode { get; }

    protected LabeledControlRowNode(TControl controlNode)
    {
        LabelNode = new LabelTextNode
        {
            String = string.Empty,
            TextFlags = TextFlags.Ellipsis,
        };
        LabelNode.AttachNode(this);

        ControlNode = controlNode;
        ControlNode.AttachNode(this);

        Height = 28.0f;
    }

    public float LabelWidth { get; set; } = 160.0f;
    public float ControlSpacing { get; set; } = 8.0f;
    public float MaximumControlWidth { get; set; } = 360.0f;

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            LabelNode.IsVisible = value;
            ControlNode.IsVisible = value;
            OnVisibilityChanged(value);
        }
    }

    public override ReadOnlySeString TextTooltip
    {
        get => ControlNode.TextTooltip;
        set => ControlNode.TextTooltip = value;
    }

    public ReadOnlySeString LabelText
    {
        get => LabelNode.String;
        set => LabelNode.String = value;
    }

    public Vector4 LabelTextColor
    {
        get => LabelNode.TextColor;
        set => LabelNode.TextColor = value;
    }

    public TextFlags LabelTextFlags
    {
        get => LabelNode.TextFlags;
        set => LabelNode.TextFlags = value;
    }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        if (ControlNode is ComponentNode componentNode)
        {
            yield return ConfigurationNavigationTarget.From(componentNode);
        }
        else if (ControlNode is IControllerNavigable navigable)
        {
            yield return ConfigurationNavigationTarget.From(ControlNode, navigable);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        var labelWidth = Math.Min(LabelWidth, Width);
        var controlX = Math.Min(Width, labelWidth + ControlSpacing);
        var availableControlWidth = Math.Max(0.0f, Width - controlX);
        var controlWidth = float.IsPositiveInfinity(MaximumControlWidth)
            ? availableControlWidth
            : Math.Min(MaximumControlWidth, availableControlWidth);

        LabelNode.Position = Vector2.Zero;
        LabelNode.Size = new Vector2(labelWidth, Height);

        ControlNode.Position = new Vector2(controlX, 0.0f);
        ControlNode.Size = new Vector2(controlWidth, Height);
    }

    protected virtual void OnVisibilityChanged(bool isVisible) { }
}
