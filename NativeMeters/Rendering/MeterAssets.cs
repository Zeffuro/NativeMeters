using System.Numerics;
using NativeMeters.Configuration;

namespace NativeMeters.Rendering;

public readonly record struct MeterAssetDescriptor(string TexturePath, Vector2 TextureCoordinates, Vector2 TextureSize, bool ResolveTheme = false);

public static class MeterAssets
{
    public static bool TryGet(MeterAsset asset, out MeterAssetDescriptor descriptor)
    {
        if (asset == MeterAsset.DutyClock)
        {
            descriptor = new MeterAssetDescriptor("ui/uld/ToDoList.tex", Vector2.Zero, new Vector2(24));
            return true;
        }

        descriptor = default;
        return false;
    }
}
