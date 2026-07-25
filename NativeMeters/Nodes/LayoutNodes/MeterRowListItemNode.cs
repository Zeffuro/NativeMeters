using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.Components;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowListItemNode : ListItemNode<CombatantRowData>, IListItemNode
{
    private readonly DynamicNodeList dynamicNodeList;

    public static MeterSettings? PrebuildSettings;
    private MeterSettings? cachedSettings;
    private List<ComponentSettings>? cachedSortedComponents;
    private int lastComponentHash;

    private MeterSettings? MeterSettings => ItemData?.Settings ?? cachedSettings;
    private Combatant? Combatant => ItemData?.Combatant;

    public static float ItemHeight => HeightHint;

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
        int hash = CalculateComponentStructureHash(components);
        if (cachedSortedComponents == null || lastComponentHash != hash)
        {
            cachedSortedComponents = components.OrderBy(s => s.ZIndex).ToList();
            lastComponentHash = hash;
        }

        if (MeterSettings.IsClickthrough)
        {
            RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision);
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
            MeterComponentType.ProgressBar => settings.ProgressBarType switch {
                ProgressBarType.Cast => new ProgressBarCastGaugeNode(),
                ProgressBarType.EnemyCast => new ProgressBarEnemyCastGaugeNode(),
                ProgressBarType.ToDo => new ProgressBarToDoGaugeNode(),
                ProgressBarType.PartyListHp => new ProgressBarPartyListHpNode(),
                ProgressBarType.LimitBreak => new ProgressBarLimitBreakGaugeNode(),
                ProgressBarType.CastLegacy => new LegacyCastProgressBarNode(),
                ProgressBarType.ToDoLegacy => new LegacyToDoProgressBarNode(),
                ProgressBarType.EnemyCastLegacy => new LegacyEnemyCastProgressBarNode(),
                _ => new ProgressBarToDoGaugeNode()
            },
            MeterComponentType.Text => new BackgroundTextNode(),
            MeterComponentType.Background => new SimpleNineGridNode {
                TexturePath = "ui/uld/ToolTipS.tex",
                TextureCoordinates = new Vector2(0.0f, 0.0f),
                TextureSize = new Vector2(32.0f, 24.0f),
                TopOffset = 10, BottomOffset = 10, LeftOffset = 15, RightOffset = 15,
            },
            MeterComponentType.Separator => new HorizontalLineNode(),
            _ => new ResNode()
        };

        return node;
    }

    private static int CalculateComponentStructureHash(IEnumerable<ComponentSettings> components)
    {
        HashCode hash = new();

        foreach (var component in components)
        {
            hash.Add(component.Id);
            hash.Add(component.ZIndex);
            hash.Add(component.Type);
            hash.Add(component.ProgressBarType);
        }

        return hash.ToHashCode();
    }

    private void UpdateComponentData(NodeBase node, ComponentSettings settings)
    {
        if (Combatant == null || MeterSettings == null) return;

        ComponentRenderer.Update(node, settings, Width, Combatant, MeterSettings);
        MeterComponentInteractions.ApplyClickthrough(node, MeterSettings.IsClickthrough);
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        dynamicNodeList.Dispose();
        base.Dispose(isNativeDestructor);
    }
}
