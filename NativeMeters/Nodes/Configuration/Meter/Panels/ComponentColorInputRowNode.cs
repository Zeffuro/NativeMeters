using System;
using System.Numerics;
using AetherBags.Nodes.Color;
using NativeMeters.Nodes.Color;

namespace NativeMeters.Nodes.Configuration.Meter.Panels;

internal sealed class ComponentColorInputRowNode : ComponentEditorRowNode<ColorPreviewButtonNode>
{
    private const float PreviewSize = 28.0f;

    public ComponentColorInputRowNode() : base(new ColorPreviewButtonNode())
    {
        ControlNode.OnClick = OpenColorPicker;
        MaximumControlWidth = PreviewSize;
    }

    public string Label
    {
        get;
        set
        {
            field = value;
            LabelText = value;
        }
    } = string.Empty;

    public Vector4 CurrentColor
    {
        get;
        set
        {
            field = value;
            ControlNode.Color = value;
        }
    }

    public Vector4 DefaultColor { get; set; }
    public Action<Vector4>? OnColorConfirmed { get; set; }
    public Action<Vector4>? OnColorCanceled { get; set; }
    public Action<Vector4>? OnColorPreviewed { get; set; }

    protected override void LayoutRow(float labelWidth, float controlX, float controlWidth)
    {
        var previewSize = Math.Min(controlWidth, Height);
        LabelNode.Position = Vector2.Zero;
        ConfigurationLayout.SetSize(LabelNode, new Vector2(labelWidth, Height));
        ControlNode.Position = new Vector2(controlX, (Height - previewSize) / 2);
        ConfigurationLayout.SetSize(ControlNode, new Vector2(previewSize));
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
