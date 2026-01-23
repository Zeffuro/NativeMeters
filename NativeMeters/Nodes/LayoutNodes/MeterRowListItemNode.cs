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

    public override float ItemHeight => 36.0f;

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

        node.IsVisible = true;
        node.Size = settings.Size;

        float x = settings.AlignmentType switch {
            AlignmentType.Right => Width - settings.Size.X - settings.Position.X,
            AlignmentType.Center => (Width / 2.0f) - (settings.Size.X / 2.0f) + settings.Position.X,
            _ => settings.Position.X
        };
        node.Position = settings.Position with { X = x };

        switch (node)
        {
            case BackgroundTextNode textNode:
                textNode.String = TagProcessor.Process(settings.DataSource, Combatant);
                textNode.FontSize = (int)settings.FontSize;
                textNode.FontType = settings.FontType;
                textNode.TextFlags = settings.TextFlags;
                textNode.AlignmentType = settings.AlignmentType;
                textNode.TextColor = settings.UseJobColor ? Combatant.GetColor() : settings.TextColor;
                textNode.TextOutlineColor = settings.TextOutlineColor;
                textNode.BackgroundColor = settings.TextBackgroundColor;
                textNode.ShowBackground = settings.ShowBackground;
                break;

            case IconImageNode iconNode:
                iconNode.IconId = Combatant.GetIconId(settings.JobIconType);
                break;

            case ProgressNode progressNode:
                var statName = string.IsNullOrWhiteSpace(settings.DataSource) ? MeterSettings.StatToTrack : settings.DataSource;
                var selector = CombatantStatHelpers.GetStatSelector(statName);
                double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
                progressNode.Progress = MeterUtil.CalculateProgressRatio(selector(Combatant), maxStat > 0 ? maxStat : 1.0);
                progressNode.BarColor = settings.UseJobColor ? Combatant.GetColor() : settings.TextColor;
                break;

            case NineGridNode backgroundNode:
                backgroundNode.Color = settings.TextColor;
                break;
        }
    }
}
