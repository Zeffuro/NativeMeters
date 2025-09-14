using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;
using NativeMeters.Classes;

namespace NativeMeters.Addons.Config;

public unsafe class ConfigAddon : NativeAddon {
    private ScrollingAreaNode<VerticalListNode>? configurationListNode;

    private readonly List<ConfigCategory> configCategories = [];

    public required ISavable Config { get; init; }

    protected override void OnSetup(AtkUnitBase* addon) {
        configurationListNode = new ScrollingAreaNode<VerticalListNode> {
            Size = ContentSize,
            Position = ContentStartPosition,
            IsVisible = true,
            ContentHeight = ContentSize.Y,
        };
        AttachNode(configurationListNode);

        foreach (var category in configCategories) {
            var listNode = category.BuildNode();
            configurationListNode.ContentNode.AddNode(listNode);

            listNode.Width = configurationListNode.ContentNode.Width;
            listNode.RecalculateLayout();
        }

        configurationListNode.ContentHeight = configurationListNode.ContentNode.Nodes.Sum(node => node.Height);
    }

    public ConfigCategory AddCategory(string label) {
        var newCategory = new ConfigCategory {
            CategoryLabel = label,
            ConfigObject = Config,
        };

        configCategories.Add(newCategory);
        return newCategory;
    }
}
