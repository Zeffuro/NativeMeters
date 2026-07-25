using System;
using System.Collections.Generic;
using KamiToolKit.BaseTypes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Services;

namespace NativeMeters.Rendering;

public class DynamicNodeList(NodeBase parentNode) : IDisposable
{
    private readonly Dictionary<string, NodeBase> _componentMap = [];
    private readonly Dictionary<string, int> componentStructureMap = [];

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
            if (!componentStructureMap.TryGetValue(component.Id, out var signature)) return true;
            if (signature != GetStructureSignature(component)) return true;
        }
        return false;
    }

    private void RebuildStructure(List<ComponentSettings> settings, Func<ComponentSettings, NodeBase> factory)
    {
        Service.Logger.Debug("Rebuilding node structure for component list");
        _componentMap.DisposeValuesLater();
        componentStructureMap.Clear();

        foreach (var setting in settings)
        {
            var node = factory(setting);
            node.AttachNode(parentNode);
            _componentMap[setting.Id] = node;
            componentStructureMap[setting.Id] = GetStructureSignature(setting);
        }
    }

    private static int GetStructureSignature(ComponentSettings component)
        => HashCode.Combine(component.Type, component.ZIndex, component.ProgressBarType);

    public void Dispose()
    {
        _componentMap.DisposeValuesLater();
        componentStructureMap.Clear();
    }
}
