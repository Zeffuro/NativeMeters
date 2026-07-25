using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using Lumina.Text.ReadOnly;
using NativeMeters.Nodes.Configuration;

namespace NativeMeters.Nodes.Input;

internal class CheckboxRowNode : ResNode, IConfigurationNavigationNode
{
    private readonly CheckboxNode checkboxNode;

    public CheckboxRowNode()
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

    public bool IsEnabled
    {
        get => checkboxNode.IsEnabled;
        set
        {
            var alpha = value ? 1.0f : 0.45f;
            checkboxNode.IsEnabled = value;
            Alpha = alpha;
            checkboxNode.Alpha = alpha;
            checkboxNode.BoxBackground.Alpha = alpha;
            checkboxNode.BoxForeground.Alpha = alpha;
            checkboxNode.Label.Alpha = alpha;
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
        set
        {
            checkboxNode.IsChecked = value;
            checkboxNode.BoxForeground.IsVisible = IsVisible && value;
        }
    }

    public Action<bool>? OnClick
    {
        get => checkboxNode.OnClick;
        set => checkboxNode.OnClick = value;
    }

    public override ReadOnlySeString TextTooltip
    {
        get => checkboxNode.TextTooltip;
        set => checkboxNode.TextTooltip = value;
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
