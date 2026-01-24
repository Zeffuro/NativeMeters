using System.Numerics;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes;

public enum RoleType { Tank = 0, Healer = 1, DPS = 2, Other = 3 }

public sealed class RoleIconNode : SimpleImageNode
{
    public RoleIconNode()
    {
        TexturePath = "ui/uld/ConfigCharacterHud_hr1.tex";
        TextureSize = new Vector2(20.0f, 20.0f);
        Size = new Vector2(20.0f, 20.0f);
    }

    public RoleType Role
    {
        set => TextureCoordinates = new Vector2((float)value * 20.0f, 0.0f);
    }
}
