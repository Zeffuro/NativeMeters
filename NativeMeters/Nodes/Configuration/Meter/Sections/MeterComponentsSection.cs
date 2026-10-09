using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public enum ComponentTarget { Header, Row, Footer }

public sealed class MeterComponentsSection : MeterConfigSection
{
    private readonly Action onLayoutChanged;
    private readonly ComponentTarget target;
    private readonly ConfigurationVerticalListNode componentsList;
    private readonly HorizontalListNode addRow;
    private readonly StringDropDownNode typeDropdown;
    private readonly List<Func<List<ComponentSettings>, bool>> pendingEdits = [];
    private List<ComponentSettings>? pendingTarget;
    private IFramework? editFramework;
    private MeterSettings? displayedMeter;
    private List<ComponentSettings>? displayedTarget;
    private readonly Dictionary<string, bool> componentCollapsedStates = [];

    public MeterComponentsSection(Func<MeterSettings> getSettings, Action onLayoutChanged, ComponentTarget target) : base(getSettings)
    {
        this.onLayoutChanged = onLayoutChanged;
        this.target = target;
        ContentItemSpacing = 4.0f;

        componentsList = new ConfigurationVerticalListNode
        {
            FitContents = true,
            FitWidth = true,
            ItemSpacing = 4.0f,
        };

        addRow = new HorizontalListNode
        {
            Size = new Vector2(Width, 32),
            ItemSpacing = 8.0f,
        };

        typeDropdown = new StringDropDownNode {
            Size = new Vector2(150, 28),
            Options = Enum.GetNames<MeterComponentType>().ToList(),
        };
        addRow.AddNode(typeDropdown);

        addRow.AddNode(new TextButtonNode {
            String = "Add Component",
            Size = new Vector2(120, 28),
            OnClick = () => AddNewComponent(typeDropdown.SelectedOption)
        });

        AddNode([componentsList, addRow]);
    }

    private List<ComponentSettings> TargetList => target switch
    {
        ComponentTarget.Header => Settings.HeaderComponents,
        ComponentTarget.Row => Settings.RowComponents,
        ComponentTarget.Footer => Settings.FooterComponents,
        _ => Settings.RowComponents
    };

    private void AddNewComponent(string? selectedComponent)
    {
        if (!Enum.TryParse<MeterComponentType>(selectedComponent, out var parsedComponentType)) return;

        QueueEdit(components =>
        {
            var component = new ComponentSettings {
                Id = Guid.NewGuid().ToString(),
                Type = parsedComponentType,
                Name = $"New {parsedComponentType}",
                Size = new Vector2(100, 20),
                Position = Vector2.Zero,
                ZIndex = components.Count > 0 ? components.Max(c => c.ZIndex) + 1 : 0
            };

            if (component.Type is MeterComponentType.Icon or MeterComponentType.JobIcon or MeterComponentType.MenuButton) component.Size = new Vector2(24);
            if (component.Type == MeterComponentType.ProgressBar) component.DataSource = StatSelector.DefaultStatSelector;

            components.Add(component);
            return true;
        });
    }

    private void QueueEdit(Func<List<ComponentSettings>, bool> edit)
    {
        if (IsDisposed || Service.Framework.IsFrameworkUnloading) return;

        if (editFramework is null)
        {
            pendingTarget = TargetList;
            editFramework = Service.Framework;
            editFramework.Update += ApplyPendingEdits;
        }

        pendingEdits.Add(edit);
    }

    private void ApplyPendingEdits(IFramework framework)
    {
        var components = pendingTarget;
        var edits = pendingEdits.ToArray();
        CancelPendingEdits();

        if (IsDisposed || framework.IsFrameworkUnloading || components is null
            || !ReferenceEquals(components, TargetList)) return;

        try
        {
            var changed = false;
            foreach (var edit in edits) changed |= edit(components);
            if (!changed) return;

            Refresh(reuseExisting: true);
            System.OverlayManager.Setup();
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to apply NativeMeters component edit.");
        }
    }

    private void CancelPendingEdits()
    {
        if (editFramework is not null) editFramework.Update -= ApplyPendingEdits;
        editFramework = null;
        pendingTarget = null;
        pendingEdits.Clear();
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        CancelPendingEdits();
        base.Dispose(isNativeDestructor);
    }

    private void RefreshSectionLayout()
    {
        RecalculateSectionLayout();
        onLayoutChanged();
    }

    public override void Refresh() => Refresh(reuseExisting: false);

    private void Refresh(bool reuseExisting)
    {
        CancelPendingEdits();
        var components = TargetList;
        if (!ReferenceEquals(displayedMeter, Settings) || !ReferenceEquals(displayedTarget, components))
        {
            reuseExisting = false;
            componentCollapsedStates.Clear();
            displayedMeter = Settings;
            displayedTarget = components;
        }
        else
        {
            foreach (var node in componentsList.Nodes.OfType<ComponentSettingsNode>())
            {
                if (node.RowComponent is { } component)
                    componentCollapsedStates[component.Id] = node.IsCollapsed;
            }
        }

        var componentIds = components.Select(component => component.Id).ToHashSet();
        foreach (var id in componentCollapsedStates.Keys.Where(id => !componentIds.Contains(id)).ToArray())
            componentCollapsedStates.Remove(id);

        if (IsCollapsed)
        {
            IsInitialized = false;
            componentsList.Clear();
            return;
        }

        IsInitialized = true;
        var existingNodes = componentsList.Nodes.OfType<ComponentSettingsNode>().ToArray();
        foreach (var node in existingNodes) node.OnLayoutChanged = null;

        try
        {
            if (!reuseExisting) componentsList.Clear();

            var retained = reuseExisting
                ? existingNodes.Where(node => components.Any(component => ReferenceEquals(node.RowComponent, component))).ToList()
                : [];
            var nodes = new List<ComponentSettingsNode>(components.Count);
            var added = new List<ComponentSettingsNode>();
            foreach (var component in components)
            {
                var node = retained.FirstOrDefault(node => ReferenceEquals(node.RowComponent, component));
                if (node is null)
                {
                    node = CreateEditor(component);
                    added.Add(node);
                }
                else
                {
                    retained.Remove(node);
                }
                nodes.Add(node);
            }

            if (reuseExisting)
            {
                var removed = existingNodes.Except(nodes).ToArray();
                if (removed.Length > 0) componentsList.RemoveNode(removed);
            }
            if (added.Count > 0) componentsList.AddNode(added);
            if (reuseExisting && !componentsList.Nodes.SequenceEqual(nodes))
            {
                var order = nodes.Select((node, index) => (node, index)).ToDictionary(item => item.node, item => item.index);
                componentsList.ReorderNodes((left, right) => order[(ComponentSettingsNode)left].CompareTo(order[(ComponentSettingsNode)right]));
            }
        }
        finally
        {
            foreach (var node in componentsList.Nodes.OfType<ComponentSettingsNode>())
                node.OnLayoutChanged = RefreshSectionLayout;
        }

        RefreshSectionLayout();
    }

    private ComponentSettingsNode CreateEditor(ComponentSettings component)
    {
        var node = new ComponentSettingsNode
        {
            IsCollapsed = componentCollapsedStates.GetValueOrDefault(component.Id),
            RowComponent = component,
            OnChanged = () => System.OverlayManager.UpdateSettings(),
            OnDeleted = () => QueueEdit(components => components.Remove(component)),
            OnDuplicate = source => QueueEdit(components =>
            {
                var index = components.IndexOf(source);
                if (index == -1) return false;

                components.Insert(index + 1, source.DeepCopy());
                return true;
            }),
        };

        var wasExpanded = !node.IsCollapsed;
        node.OnToggle = _ =>
        {
            var isExpanded = !node.IsCollapsed;
            if (wasExpanded == isExpanded) return;

            wasExpanded = isExpanded;
            RefreshSectionLayout();
        };
        return node;
    }
}
