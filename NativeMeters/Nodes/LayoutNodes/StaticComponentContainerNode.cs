using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Components;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class StaticComponentContainerNode : SimpleComponentNode
{
    private readonly DynamicNodeList graphManager;
    private readonly List<ComponentSettings> settingsList;

    public MeterSettings? MeterSettings
    {
        get;
        set;
    }

    public StaticComponentContainerNode(List<ComponentSettings> settings)
    {
        settingsList = settings;
        graphManager = new DynamicNodeList(this);
        DisableCollisionNode = true;
    }

    public void Update()
    {
        var sortedSettings = settingsList.OrderBy(s => s.ZIndex).ToList();

        graphManager.Update(sortedSettings, CreateComponent);

        if (settingsList.Count == 0 && CreatedNodes.Count == 0)
        {
            var guard = new SimpleComponentNode { Height = 0 };
            guard.AttachNode(this);
        }

        foreach (var settings in sortedSettings)
        {
            if (graphManager.Components.TryGetValue(settings.Id, out var node))
            {
                UpdateComponentData(node, settings);
            }
        }
    }

    private NodeBase CreateComponent(ComponentSettings settings)
    {
        NodeBase node = settings.Type switch {
            MeterComponentType.JobIcon or MeterComponentType.Icon => new IconImageNode { FitTexture = true },
            MeterComponentType.ProgressBar => new ProgressBarNode(),
            MeterComponentType.Text => new BackgroundTextNode(),
            MeterComponentType.Background => new SimpleNineGridNode {
                TexturePath = "ui/uld/ToolTipS.tex",
                TextureCoordinates = Vector2.Zero,
                TextureSize = new Vector2(32.0f, 24.0f),
                TopOffset = 10, BottomOffset = 10, LeftOffset = 15, RightOffset = 15,
            },
            MeterComponentType.MenuButton => new HeaderMenuButtonNode{ MeterSettings = MeterSettings},
            MeterComponentType.Separator => new HorizontalLineNode(),
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
        if (encounter == null)
        {
            encounter = new Encounter
            {
                Title = "No Encounter",
            };
        }

        ComponentRenderer.Update(node, settings, Width, encounter);
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        graphManager.Dispose();
        base.Dispose(disposing, isNativeDestructor);
    }
}
