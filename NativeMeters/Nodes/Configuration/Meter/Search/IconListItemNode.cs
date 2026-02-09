using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Meter.Search;

public class IconListItemNode : ListItemNode<uint>
{
    public override float ItemHeight => 40.0f;

    private readonly IconImageNode iconNode;
    private readonly TextNode idTextNode;

    public IconListItemNode()
    {
        iconNode = new IconImageNode {
            FitTexture = true,
            Size = new Vector2(32, 32),
            Position = new Vector2(4, 4)
        };
        iconNode.AttachNode(this);

        idTextNode = new TextNode {
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(8),
            Position = new Vector2(45, 0),
        };
        idTextNode.AttachNode(this);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        idTextNode.Size = new Vector2(Width - 50, Height);
    }

    protected override void SetNodeData(uint iconId)
    {
        iconNode.IconId = iconId;
        idTextNode.String = $"Icon ID: {iconId}";
    }
}
