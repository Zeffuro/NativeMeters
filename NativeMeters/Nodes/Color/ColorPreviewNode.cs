using System.Drawing;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Color;

public class ColorPreviewNode : ResNode
{
    private readonly BackgroundImageNode colorBackground;
    private readonly ImGuiImageNode alphaLayer;
    private readonly BackgroundImageNode colorForeground;

    private bool isDisposed;

    public ColorPreviewNode()
    {
        base.Size = new Vector2(64, 64);

        colorBackground = new BackgroundImageNode
        {
            IsVisible = true,
            Color = KnownColor.Black.Vector(),
            FitTexture = true,
        };
        colorBackground.AttachNode(this);

        alphaLayer = new ImGuiImageNode
        {
            IsVisible = true,
            TexturePath = GetAlphaTexturePath(),
            WrapMode = WrapMode.Stretch,
        };
        alphaLayer.AttachNode(this);

        colorForeground = new BackgroundImageNode
        {
            IsVisible = true,
            Color = KnownColor.White.Vector(),
            FitTexture = true,
        };
        colorForeground.AttachNode(this);

        UpdateLayout();
    }

    public override Vector4 Color
    {
        get => colorForeground.Color;
        set => colorForeground.Color = value;
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

    public BackgroundImageNode BackgroundNode => colorBackground;
    public BackgroundImageNode ForegroundNode => colorForeground;

    private void UpdateLayout()
    {
        const float backgroundPadding = 6f;
        const float alphaPadding = 8f;
        const float foregroundPadding = 8f;

        var bgSize = base.Size - new Vector2(backgroundPadding * 2f);
        var alphaSize = base.Size - new Vector2(alphaPadding * 2f);
        var fgSize = base.Size - new Vector2(foregroundPadding * 2f);

        colorBackground.Size = bgSize;
        colorBackground.Position = new Vector2(backgroundPadding, backgroundPadding);

        alphaLayer.Size = alphaSize;
        alphaLayer.Position = new Vector2(alphaPadding, alphaPadding);

        colorForeground.Size = fgSize;
        colorForeground.Position = new Vector2(foregroundPadding, foregroundPadding);
    }

    private static string GetAlphaTexturePath()
    {
        var baseDir = Service.PluginInterface.AssemblyLocation.Directory!.FullName;
        return Path.Combine(baseDir, "Assets", "alpha_background.png");
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        if (isDisposed)
        {
            base.Dispose(disposing, isNativeDestructor);
            return;
        }

        isDisposed = true;
        if (disposing)
        {
            colorBackground.Dispose();
            alphaLayer.Dispose();
            colorForeground.Dispose();
        }

        base.Dispose(disposing, isNativeDestructor);
    }
}
