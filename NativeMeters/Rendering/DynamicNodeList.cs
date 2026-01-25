using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;

namespace NativeMeters.Rendering;

public class DynamicNodeList(NodeBase parentNode) : IDisposable
{
    private readonly Dictionary<string, NodeBase> _componentMap = [];

    public IReadOnlyDictionary<string, NodeBase> Components => _componentMap;

    public void Update(List<ComponentSettings> settings, Func<ComponentSettings, NodeBase> factory)
    {
        if (!HasStructureChanged(settings)) return;
        RebuildStructure(settings, factory);
    }

    private bool HasStructureChanged(List<ComponentSettings> settings)
    {
        if (_componentMap.Count != settings.Count) return true;

        foreach (var component in settings)
        {
            if (!_componentMap.ContainsKey(component.Id)) return true;
        }
        return false;
    }

    private void RebuildStructure(List<ComponentSettings> settings, Func<ComponentSettings, NodeBase> factory)
    {
        _componentMap.DisposeValues();

        var sorted = settings.OrderBy(c => c.ZIndex).ToList();

        foreach (var setting in sorted)
        {
            var node = factory(setting);
            node.AttachNode(parentNode);
            _componentMap[setting.Id] = node;
        }
    }

    public void Dispose()
    {
        _componentMap.DisposeValues();
    }
}
