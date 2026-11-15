using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.Components;

public sealed class AssetImageNode : SimpleImageNode
{
    private MeterAsset? appliedAsset;
    private bool hasValidAsset;

    public AssetImageNode()
    {
        Size = new Vector2(24);
        ImageNodeFlags = 0;
        WrapMode = WrapMode.Stretch;
        IsVisible = false;
    }

    public bool ApplyAsset(MeterAsset asset)
    {
        if (appliedAsset == asset) return hasValidAsset;

        appliedAsset = asset;
        hasValidAsset = MeterAssets.TryGet(asset, out var descriptor);
        if (!hasValidAsset)
        {
            TexturePath = string.Empty;
            return false;
        }

        TextureCoordinates = descriptor.TextureCoordinates;
        TextureSize = descriptor.TextureSize;
        LoadTexture(descriptor.TexturePath, resolveTheme: descriptor.ResolveTheme);
        MarkDirty();
        return true;
    }
}
