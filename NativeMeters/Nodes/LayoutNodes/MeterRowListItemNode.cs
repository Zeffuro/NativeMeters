using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Helpers;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowListItemNode : ListItemNode<CombatantRowData>
{
    private readonly Dictionary<string, NodeBase> componentMap = new();
    private int lastComponentCount = -1;

    private MeterSettings? MeterSettings => ItemData?.Settings;
    private Combatant? Combatant => ItemData?.Combatant;

    public override float ItemHeight => MeterSettings?.RowHeight ?? HeightHint;

    public static float HeightHint = 36.0f;

    public MeterRowListItemNode()
    {
        DisableInteractions();
    }

    protected override void SetNodeData(CombatantRowData itemData) { }

    public override void Update()
    {
        if (ItemData == null || MeterSettings == null || Combatant == null) return;

        if (lastComponentCount != MeterSettings.RowComponents.Count)
        {
            RebuildStructure();
            lastComponentCount = MeterSettings.RowComponents.Count;
        }

        foreach (var settings in MeterSettings.RowComponents)
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

        var sorted = MeterSettings!.RowComponents.OrderBy(c => c.ZIndex).ToList();

        foreach (var settings in sorted)
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
            MeterComponentType.ProgressBar => MeterSettings!.ProgressBarType switch {
                ProgressBarType.Cast => new ProgressBarCastNode(),
                ProgressBarType.EnemyCast => new ProgressBarEnemyCastNode(),
                _ => new ProgressBarNode()
            },
            MeterComponentType.Text => new BackgroundTextNode(),
            MeterComponentType.Background => new SimpleNineGridNode {
                TexturePath = "ui/uld/ToolTipS.tex",
                TextureCoordinates = new Vector2(0.0f, 0.0f),
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
        if (Combatant == null || MeterSettings == null) return;

        ComponentUpdateHelper.Update(node, settings, Width, Combatant);
    }
}
