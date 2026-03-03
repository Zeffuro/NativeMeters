using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowListItemNode : ListItemNode<CombatantRowData>
{
    private readonly DynamicNodeList dynamicNodeList;

    public static MeterSettings? PrebuildSettings;
    private MeterSettings? cachedSettings;
    private List<ComponentSettings>? cachedSortedComponents;
    private int lastComponentHash;

    private MeterSettings? MeterSettings => ItemData?.Settings ?? cachedSettings;
    private Combatant? Combatant => ItemData?.Combatant;

    public override float ItemHeight => MeterSettings?.RowHeight ?? HeightHint;

    public static float HeightHint = 36.0f;

    public MeterRowListItemNode()
    {
        dynamicNodeList = new DynamicNodeList(this);
        DisableInteractions();

        if (PrebuildSettings != null)
        {
            PreBuildStructure(PrebuildSettings);
        }
    }

    private void PreBuildStructure(MeterSettings settings)
    {
        cachedSettings = settings;
        var sortedComponents = settings.RowComponents.OrderBy(s => s.ZIndex).ToList();
        dynamicNodeList.Update(sortedComponents, CreateComponent);
    }

    protected override void SetNodeData(CombatantRowData itemData) { }

    public override void Update()
    {
        if (ItemData == null || MeterSettings == null || Combatant == null) return;
        cachedSettings = ItemData.Settings;

        var components = MeterSettings.RowComponents;
        int hash = components.Count;
        if (cachedSortedComponents == null || lastComponentHash != hash)
        {
            cachedSortedComponents = components.OrderBy(s => s.ZIndex).ToList();
            lastComponentHash = hash;
        }

        dynamicNodeList.Update(cachedSortedComponents, CreateComponent);

        foreach (var settings in cachedSortedComponents)
        {
            if (dynamicNodeList.Components.TryGetValue(settings.Id, out var node))
            {
                UpdateComponentData(node, settings);
            }
        }
    }

    private NodeBase CreateComponent(ComponentSettings settings)
    {
        NodeBase node = settings.Type switch {
            MeterComponentType.JobIcon or MeterComponentType.Icon => new IconImageNode { FitTexture = true },
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
        if (Combatant == null || MeterSettings == null) return;

        ComponentRenderer.Update(node, settings, Width, Combatant);
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        dynamicNodeList.Dispose();
        base.Dispose(disposing, isNativeDestructor);
    }
}
