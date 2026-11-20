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
using NativeMeters.Nodes.Components.ProgressBars;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class StaticComponentContainerNode : ResNode
{
    private readonly DynamicNodeList graphManager;
    private readonly List<ComponentSettings> settingsList;
    private List<ComponentSettings>? cachedSortedSettings;
    private int lastSettingsHash;
    private ResNode? emptyGuard;

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

    public void Update(Encounter? encounter = null)
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
            if (emptyGuard == null)
            {
                emptyGuard = new ResNode { Height = 0 };
                emptyGuard.AttachNode(this);
            }
        }
        else if (emptyGuard != null)
        {
            emptyGuard.DisposeLater();
            emptyGuard = null;
        }

        encounter ??= System.ActiveMeterService.GetEncounter() ?? EmptyEncounter;
        foreach (var settings in cachedSortedSettings)
        {
            if (graphManager.Components.TryGetValue(settings.Id, out var node))
            {
                ComponentRenderer.Update(node, settings, Width, encounter, MeterSettings);
                MeterComponentInteractions.ApplyClickthrough(node, MeterSettings?.IsClickthrough == true);
            }
        }
    }

    private NodeBase CreateComponent(ComponentSettings settings)
    {
        NodeBase node = settings.Type switch {
            MeterComponentType.JobIcon or MeterComponentType.Icon => new IconImageNode { FitTexture = true },
            MeterComponentType.Asset => new AssetImageNode(),
            MeterComponentType.ProgressBar => MeterProgressNodes.Create(settings.ProgressBarType, settings.PreserveBarEnds),
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
            hash.Add(component.PreserveBarEnds);
        }

        return hash.ToHashCode();
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        graphManager.Dispose();
        base.Dispose(isNativeDestructor);
    }
}
