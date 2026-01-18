using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class StaticComponentContainerNode : SimpleComponentNode
{
    private readonly Dictionary<string, NodeBase> componentMap = new();
    private readonly List<ComponentSettings> settingsList;
    private int lastComponentCount = -1;

    public StaticComponentContainerNode(List<ComponentSettings> settings)
    {
        settingsList = settings;
        DisableCollisionNode = true;
    }

    public void Update()
    {
        if (lastComponentCount != settingsList.Count)
        {
            RebuildStructure();
            lastComponentCount = settingsList.Count;
        }

        foreach (var settings in settingsList)
        {
            if (componentMap.TryGetValue(settings.Id, out var node))
            {
                UpdateComponentData(node, settings);
            }
        }
    }

    private void RebuildStructure()
    {
        foreach (var node in componentMap.Values) node.Dispose();
        componentMap.Clear();

        var sortedComponents = settingsList.OrderBy(component => component.ZIndex).ToList();

        foreach (var settings in sortedComponents)
        {
            var node = CreateComponent(settings);
            node.AttachNode(this);
            componentMap[settings.Id] = node;
        }
    }

    private NodeBase CreateComponent(ComponentSettings settings)
    {
        // Reusing the factory logic from RowListItem
        NodeBase node = settings.Type switch {
            MeterComponentType.JobIcon => new IconImageNode { FitTexture = true },
            MeterComponentType.ProgressBar => new ProgressBarNode(),
            MeterComponentType.Text => new BackgroundTextNode(),
            MeterComponentType.Background => new SimpleNineGridNode {
                TexturePath = "ui/uld/ToolTipS.tex",
                TextureCoordinates = Vector2.Zero,
                TextureSize = new Vector2(32.0f, 24.0f),
                TopOffset = 10, BottomOffset = 10, LeftOffset = 15, RightOffset = 15,
            },
            _ => new SimpleComponentNode()
        };

        if (node is SimpleComponentNode simpleNode)
        {
            simpleNode.DisableCollisionNode = true;
        }

        return node;
    }

    private void UpdateComponentData(NodeBase node, ComponentSettings settings)
    {
        node.IsVisible = true;
        node.Position = settings.Position;
        node.Size = settings.Size;

        if (node is BackgroundTextNode textNode)
        {
            //textNode.String = EncounterStatHelpers.GetGlobalStatValue(settings.DataSource);
            textNode.FontSize = (int)settings.FontSize;
            textNode.FontType = settings.FontType;
            textNode.TextFlags = settings.TextFlags;
            textNode.TextColor = settings.TextColor;
            textNode.ShowBackground = settings.ShowBackground;
        }
    }
}