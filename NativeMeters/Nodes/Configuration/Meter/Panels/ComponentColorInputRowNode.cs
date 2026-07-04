using System;
using System.Collections.Generic;
using System.Numerics;
using AetherBags.Nodes.Color;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Color;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentColorInputRowNode : ResNode, IConfigurationNavigationNode
{
    private const float PreviewSize = 28.0f;
    private const float LabelOffset = PreviewSize + 4.0f;

    private readonly ColorPreviewButtonNode colorPreview;
    private readonly LabelTextNode labelTextNode;

    public ComponentColorInputRowNode()
    {
        colorPreview = new ColorPreviewButtonNode
        {
            Size = new Vector2(PreviewSize),
            OnClick = OpenColorPicker,
        };
        colorPreview.AttachNode(this);

        labelTextNode = new LabelTextNode
        {
            TextFlags = TextFlags.Ellipsis,
        };
        labelTextNode.AttachNode(this);

        Height = 28.0f;
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            colorPreview.IsVisible = value;
            labelTextNode.IsVisible = value;
        }
    }

    public string Label
    {
        get;
        set
        {
            field = value;
            labelTextNode.String = value;
        }
    } = string.Empty;

    public Vector4 CurrentColor
    {
        get;
        set
        {
            field = value;
            colorPreview.Color = value;
        }
    }

    public Vector4 DefaultColor { get; set; }
    public Action<Vector4>? OnColorConfirmed { get; set; }
    public Action<Vector4>? OnColorCanceled { get; set; }
    public Action<Vector4>? OnColorPreviewed { get; set; }

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        yield return ConfigurationNavigationTarget.From(colorPreview);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        colorPreview.Position = Vector2.Zero;
        colorPreview.Size = new Vector2(Math.Min(PreviewSize, Height));

        labelTextNode.Position = new Vector2(LabelOffset, 0.0f);
        labelTextNode.Size = new Vector2(Math.Max(0.0f, Width - LabelOffset), Height);
    }

    private void OpenColorPicker()
    {
        ColorInputRow.OpenSharedColorPicker(
            CurrentColor,
            DefaultColor,
            color => CurrentColor = color,
            OnColorConfirmed,
            OnColorPreviewed,
            OnColorCanceled);
    }
}
