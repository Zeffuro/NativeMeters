using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Meter.Tags;

public class TagListItemNode : ListItemNode<TagInfo>
{
    public override float ItemHeight => 46.0f;

    private readonly TextNode tagTextNode;
    private readonly TextNode exampleTextNode;
    private readonly TextNode descTextNode;
    private readonly TextButtonNode insertButton;

    public TagListItemNode()
    {
        tagTextNode = new TextNode {
            FontSize = 14,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(66),
            Position = new Vector2(4, 4),
        };
        tagTextNode.AttachNode(this);

        exampleTextNode = new TextNode {
            FontSize = 14,
            AlignmentType = AlignmentType.Right,
            TextColor = ColorHelper.GetColor(1),
            Position = new Vector2(0, 4),
        };
        exampleTextNode.AttachNode(this);

        descTextNode = new TextNode {
            FontSize = 12,
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(3),
            Position = new Vector2(4, 24),
        };
        descTextNode.AttachNode(this);

        insertButton = new TextButtonNode {
            String = "Insert",
            Size = new Vector2(60, 24),
            OnClick = () => {
                if (ItemData != null)
                    System.TagSearchAddon.OnInsertClicked?.Invoke(ItemData.Tag);
            }
        };
        insertButton.AttachNode(this);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        insertButton.Position = new Vector2(Width - 65, (Height - 24) / 2);

        exampleTextNode.Position = new Vector2(Width - 195, 4);
        exampleTextNode.Size = new Vector2(120, 18);

        tagTextNode.Position = new Vector2(4, 4);
        tagTextNode.Size = new Vector2(Width - 205, 18);

        descTextNode.Position = new Vector2(4, 24);
        descTextNode.Size = new Vector2(Width - 75, 18);
    }

    protected override void SetNodeData(TagInfo info)
    {
        tagTextNode.String = info.Tag;
        exampleTextNode.String = $"ex: {info.Example}";
        descTextNode.String = info.Description;
    }
}
