using System;
using System.Numerics;
using AetherBags.Nodes.Color;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Premade.Addons;
using KamiToolKit.Premade.Color;

namespace NativeMeters.Nodes.Color;

public class ColorInputRow : HorizontalListNode
{
    private static ColorPickerAddon? _sharedColorPickerAddon;
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

            if (_sharedColorPickerAddon is not null)
            {
                _sharedColorPickerAddon.InitialColor = snapshot;
                _sharedColorPickerAddon.DefaultColor = DefaultColor;
                _sharedColorPickerAddon.Toggle();

                _sharedColorPickerAddon.OnColorConfirmed = color =>
                {
                    CurrentColor = color;
                    node.Color = color;
                    OnColorConfirmed?.Invoke(color);
                };

                _sharedColorPickerAddon.OnColorPreviewed = color =>
                {
                    node.Color = color;
                    OnColorPreviewed?.Invoke(color);
                };

                _sharedColorPickerAddon.OnColorCancelled = () =>
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
        if (_sharedColorPickerAddon is not null) return;

        _sharedColorPickerAddon = new ColorPickerAddon {
            InternalName = "ColorPicker_NativeMeters",
            Title = "Pick a color",
        };
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor) {
        base.Dispose();
    }

    public static void DisposeSharedColorPicker()
    {
        _sharedColorPickerAddon?.Dispose();
        _sharedColorPickerAddon = null;
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
    public Action<Vector4>? OnColorPreviewed { get; set; }
}
