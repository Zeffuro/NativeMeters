using System.Drawing;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Services;

namespace AetherBags.Nodes.Color;

public class ColorPreviewNode : ResNode
{
    private readonly BackgroundImageNode _colorBackground;
    private readonly ImGuiImageNode _alphaLayer;
    private readonly BackgroundImageNode _colorForeground;

    private bool _isDisposed;

    public ColorPreviewNode()
    {
        base.Size = new Vector2(64, 64);

        _colorBackground = new BackgroundImageNode
        {
            IsVisible = true,
            Color = KnownColor.Black.Vector(),
            FitTexture = true,
        };
        _colorBackground.AttachNode(this);

        _alphaLayer = new ImGuiImageNode
        {
            IsVisible = true,
            TexturePath = GetAlphaTexturePath(),
            WrapMode = WrapMode.Stretch,
        };
        _alphaLayer.AttachNode(this);

        _colorForeground = new BackgroundImageNode
        {
            IsVisible = true,
            Color = KnownColor.White.Vector(),
            FitTexture = true,
        };
        _colorForeground.AttachNode(this);

        UpdateLayout();
    }

    public override Vector4 Color
    {
        get => _colorForeground.Color;
        set => _colorForeground.Color = value;
    }

    public override Vector2 Size
    {
        get => base.Size;
        set
        {
            base.Size = value;
            UpdateLayout();
        }
    }

    public BackgroundImageNode BackgroundNode => _colorBackground;
    public BackgroundImageNode ForegroundNode => _colorForeground;

    private void UpdateLayout()
    {
        const float backgroundPadding = 6f;
        const float alphaPadding = 8f;
        const float foregroundPadding = 8f;

        var bgSize = base.Size - new Vector2(backgroundPadding * 2f);
        var alphaSize = base.Size - new Vector2(alphaPadding * 2f);
        var fgSize = base.Size - new Vector2(foregroundPadding * 2f);

        _colorBackground.Size = bgSize;
        _colorBackground.Position = new Vector2(backgroundPadding, backgroundPadding);

        _alphaLayer.Size = alphaSize;
        _alphaLayer.Position = new Vector2(alphaPadding, alphaPadding);

        _colorForeground.Size = fgSize;
        _colorForeground.Position = new Vector2(foregroundPadding, foregroundPadding);
    }

    private static string GetAlphaTexturePath()
    {
        var baseDir = Service.PluginInterface.AssemblyLocation.Directory!.FullName;
        return Path.Combine(baseDir, "Assets", "alpha_background.png");
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        if (_isDisposed)
        {
            base.Dispose(disposing, isNativeDestructor);
            return;
        }

        _isDisposed = true;
        if (disposing)
        {
            _colorBackground.Dispose();
            _alphaLayer.Dispose();
            _colorForeground.Dispose();
        }

        base.Dispose(disposing, isNativeDestructor);
    }
}
