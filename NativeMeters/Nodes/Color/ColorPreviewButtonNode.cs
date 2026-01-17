using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Color;

namespace AetherBags.Nodes.Color;

public class ColorPreviewButtonNode : ButtonBase {
    private readonly ColorPreviewNode _colorPreview;

    public ColorPreviewButtonNode() {
        _colorPreview = new ColorPreviewNode {
            IsVisible = true,
            Position = Vector2.Zero,
            Size = base.Size,
        };

        _colorPreview.AttachNode(this);

        LoadTimelines();

        InitializeComponentEvents();
    }

    public override Vector4 Color
    {
        get => _colorPreview.Color;
        set => _colorPreview.Color = value;
    }

    public override Vector2 Size
    {
        get => base.Size;
        set
        {
            base.Size = value;
            _colorPreview.Size = value;
        }
    }

    private void LoadTimelines()
        => LoadTwoPartTimelines(this, _colorPreview);
}
