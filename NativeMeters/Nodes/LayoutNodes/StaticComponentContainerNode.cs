using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.Components;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class StaticComponentContainerNode : ResNode
{
    private readonly DynamicNodeList graphManager;
    private readonly List<ComponentSettings> settingsList;
    private List<ComponentSettings>? cachedSortedSettings;
    private int lastSettingsHash;

    private static readonly Encounter EmptyEncounter = new() { Title = "No Encounter" };

    public MeterSettings? MeterSettings
    {
        get;
        set;
    }

    public StaticComponentContainerNode(List<ComponentSettings> settings)
    {
        settingsList = settings;
        graphManager = new DynamicNodeList(this);
    }

    public void Update()
    {
        var settingsHash = CalculateComponentStructureHash(settingsList);
        if (cachedSortedSettings == null || lastSettingsHash != settingsHash)
        {
            cachedSortedSettings = settingsList.OrderBy(s => s.ZIndex).ToList();
            lastSettingsHash = settingsHash;
        }

        graphManager.Update(cachedSortedSettings, CreateComponent);

        if (settingsList.Count == 0)
        {
            var guard = new ResNode { Height = 0 };
            guard.AttachNode(this);
        }

        foreach (var settings in cachedSortedSettings)
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
            MeterComponentType.ProgressBar => settings.ProgressBarType switch {
                ProgressBarType.Cast => new ProgressBarCastGaugeNode(),
                ProgressBarType.EnemyCast => new ProgressBarEnemyCastGaugeNode(),
                ProgressBarType.PartyListHp => new ProgressBarPartyListHpNode(),
                ProgressBarType.LimitBreak => new ProgressBarLimitBreakGaugeNode(),
                _ => new ProgressBarToDoGaugeNode()
            },
            MeterComponentType.Text => new BackgroundTextNode(),
            MeterComponentType.Background => new SimpleNineGridNode {
                TexturePath = "ui/uld/ToolTipS.tex",
                TextureCoordinates = Vector2.Zero,
                TextureSize = new Vector2(32.0f, 24.0f),
                TopOffset = 10, BottomOffset = 10, LeftOffset = 15, RightOffset = 15,
            },
            MeterComponentType.MenuButton => new HeaderMenuButtonNode{ MeterSettings = MeterSettings},
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
        var encounter = System.ActiveMeterService.GetEncounter() ?? EmptyEncounter;
        ComponentRenderer.Update(node, settings, Width, encounter, MeterSettings);
        MeterComponentInteractions.ApplyClickthrough(node, MeterSettings?.IsClickthrough == true);
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        graphManager.Dispose();
        base.Dispose(isNativeDestructor);
    }
}
