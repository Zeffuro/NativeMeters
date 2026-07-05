using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentCheckboxRowNode : ResNode, IConfigurationNavigationNode
{
    private readonly CheckboxNode checkboxNode;

    public ComponentCheckboxRowNode()
    {
        checkboxNode = new CheckboxNode
        {
            DisableAutoResize = true,
        };
        checkboxNode.AttachNode(this);

        Height = 28.0f;
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            checkboxNode.IsVisible = value;
            checkboxNode.BoxBackground.IsVisible = value;
            checkboxNode.BoxForeground.IsVisible = value && checkboxNode.IsChecked;
            checkboxNode.Label.IsVisible = value;
        }
    }

    public ReadOnlySeString String
    {
        get => checkboxNode.String;
        set => checkboxNode.String = value;
    }

    public bool IsChecked
    {
        get => checkboxNode.IsChecked;
        set => checkboxNode.IsChecked = value;
    }

    public Action<bool>? OnClick
    {
        get => checkboxNode.OnClick;
        set => checkboxNode.OnClick = value;
    }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        yield return ConfigurationNavigationTarget.From(checkboxNode);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        checkboxNode.Position = Vector2.Zero;
        checkboxNode.Size = Size;
    }
}
