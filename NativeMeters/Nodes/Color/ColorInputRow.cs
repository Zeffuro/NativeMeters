using System;
using System.Numerics;
using AetherBags.Nodes.Color;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Premade.Addons;

namespace NativeMeters.Nodes.Color;

public class ColorInputRow : HorizontalListNode
{
    private ColorPickerAddon? colorPickerAddon;
    private readonly LabelTextNode labelTextNode;
    private readonly ColorPreviewButtonNode colorPreview;

    public ColorInputRow()
    {
        InitializeColorPicker();

        colorPreview = new ColorPreviewButtonNode { Size = new Vector2(28) };
        labelTextNode = new LabelTextNode
        {
            TextFlags = TextFlags.AutoAdjustNodeSize,
            Position = new Vector2(28, 0),
            Height = 28,
        };

        var node = colorPreview;

        node.OnClick = () =>
        {
            var snapshot = CurrentColor;

            if (colorPickerAddon is not null)
            {
                colorPickerAddon.InitialColor = snapshot;
                colorPickerAddon.DefaultColor = DefaultColor;
                colorPickerAddon.Toggle();

                colorPickerAddon.OnColorConfirmed = color =>
                {
                    CurrentColor = color;
                    node.Color = color;
                    OnColorConfirmed?.Invoke(color);
                };

                colorPickerAddon.OnColorPreviewed = color =>
                {
                    node.Color = color;
                    OnColorPreviewed?.Invoke(color);
                };

                colorPickerAddon.OnColorCancelled = () =>
                {
                    CurrentColor = snapshot;
                    node.Color = snapshot;
                    OnColorCanceled?.Invoke(snapshot);
                };
            }
        };

        colorPreview.AttachNode(this);
        labelTextNode.AttachNode(this);
    }

    private void InitializeColorPicker() {
        if (colorPickerAddon is not null) return;

        colorPickerAddon = new ColorPickerAddon {
            InternalName = "ColorPicker_NativeMeters",
            Title = "Pick a color",
        };
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor) {
        base.Dispose();

        colorPickerAddon?.Dispose();
        colorPickerAddon = null;
    }

    public required string Label
    {
        get;
        set
        {
            field = value;
            labelTextNode.String = value;
        }
    }

    public required Vector4 CurrentColor
    {
        get;
        set
        {
            field = value;
            colorPreview.Color = value;
        }
    }

    public required Vector4 DefaultColor { get; set; }
    public Action<Vector4>? OnColorConfirmed { get; set; }
    public Action<Vector4>? OnColorCanceled { get; set; }
    public Action<Vector4>? OnColorChange { get; set; }
    public Action<Vector4>? OnColorPreviewed { get; set; }
}