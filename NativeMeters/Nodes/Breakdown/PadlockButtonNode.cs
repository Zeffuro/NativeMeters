using System.Numerics;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Breakdown;

// Thanks Kami for the PadlockButton from VanillaPlus https://github.com/MidoriKami/VanillaPlus/blob/master/VanillaPlus/Features/LockChatButton/PadlockButtonNode.cs
public sealed class PadlockButtonNode : TextureButtonNode
{
    private static readonly Vector2 LockedCoordinate = new(88.0f, 0.0f);
    private static readonly Vector2 UnlockedCoordinate = new(48.0f, 0.0f);

    public PadlockButtonNode()
    {
        TexturePath = "ui/uld/ActionBar.tex";
        TextureCoordinates = UnlockedCoordinate;
        TextureSize = new Vector2(20.0f, 24.0f);
        ImageNode.Scale = new Vector2(0.9f);
    }

    public bool IsLocked
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            TextureCoordinates = value ? LockedCoordinate : UnlockedCoordinate;
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ImageNode.Origin = ImageNode.Size / 2.0f;
    }

    protected override void ClickHandler()
    {
        IsLocked = !IsLocked;
        base.ClickHandler();
    }
}
