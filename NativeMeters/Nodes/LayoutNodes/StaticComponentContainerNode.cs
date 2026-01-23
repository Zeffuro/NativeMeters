using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

        // Since VerticalList will error out to 65532 if the list empty we add a dummy so the FitContents calculation works
        if (settingsList.Count == 0)
        {
            var guard = new SimpleComponentNode { Height = 0 };
            guard.AttachNode(this);
        }
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
        var encounter = System.ActiveMeterService.GetEncounter();
        if (encounter == null) return;

        node.IsVisible = true;
        node.Size = settings.Size;

        float x = settings.AlignmentType switch {
            AlignmentType.Right => Width - settings.Size.X - settings.Position.X,
            AlignmentType.Center => (Width / 2.0f) - (settings.Size.X / 2.0f) + settings.Position.X,
            _ => settings.Position.X
        };
        node.Position = settings.Position with { X = x };

        if (node is BackgroundTextNode textNode)
        {
            textNode.String = TagProcessor.Process(settings.DataSource, encounter);
            textNode.FontSize = (int)settings.FontSize;
            textNode.FontType = settings.FontType;
            textNode.TextFlags = settings.TextFlags;
            textNode.AlignmentType = settings.AlignmentType;
            textNode.TextColor = settings.TextColor;
            textNode.TextOutlineColor = settings.TextOutlineColor;
            textNode.BackgroundColor = settings.TextBackgroundColor;
            textNode.ShowBackground = settings.ShowBackground;
        }
    }
}
