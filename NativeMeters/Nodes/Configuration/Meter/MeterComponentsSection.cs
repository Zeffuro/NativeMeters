using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public enum ComponentTarget { Header, Row, Footer }

public sealed class MeterComponentsSection : MeterConfigSection
{
    private readonly VerticalListNode listContainer;
    private readonly HorizontalListNode addRow;
    private readonly Action onLayoutChanged;
    private readonly ComponentTarget target;

    private List<string> lastComponentIds = new();

    public MeterComponentsSection(Func<MeterSettings> getSettings, Action onLayoutChanged, ComponentTarget target) : base(getSettings)
    {
        this.onLayoutChanged = onLayoutChanged;
        this.target = target;

        listContainer = new VerticalListNode {
            ItemSpacing = 4.0f,
            FitContents = true,
        };
        AddNode(listContainer);

        addRow = new HorizontalListNode
        {
            Size = new Vector2(300, 32),
            ItemSpacing = 8.0f,
        };

        var typeDropdown = new TextDropDownNode {
            Size = new Vector2(150, 28),
            Options = Enum.GetNames<MeterComponentType>().ToList(),
        };
        addRow.AddNode(typeDropdown);

        addRow.AddNode(new TextButtonNode {
            String = "Add Component",
            Size = new Vector2(120, 28),
            OnClick = () => AddNewComponent(typeDropdown.SelectedOption)
        });

        AddNode(addRow);
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

        var component = new ComponentSettings {
            Id = Guid.NewGuid().ToString(),
            Type = parsedComponentType,
            Name = $"New {parsedComponentType}",
            Size = new Vector2(100, 20),
            Position = Vector2.Zero,
            ZIndex = TargetList.Count > 0 ? TargetList.Max(c => c.ZIndex) + 1 : 0
        };

        if (component.Type is MeterComponentType.Icon or MeterComponentType.JobIcon or MeterComponentType.MenuButton) component.Size = new Vector2(24);

        TargetList.Add(component);
        lastComponentIds.Clear();
        Refresh();
        System.OverlayManager.Setup();
    }

    private void RefreshLayout()
    {
        listContainer.RecalculateLayout();
        ContentNode.RecalculateLayout();
        RecalculateLayout();
        onLayoutChanged();
    }

    public override void Refresh()
    {
        var currentIds = TargetList.Select(c => c.Id).ToList();
        bool componentsChanged = !currentIds.SequenceEqual(lastComponentIds);

        if (!componentsChanged && IsCollapsed)
        {
            return;
        }

        if (!componentsChanged && !IsCollapsed)
        {
            return;
        }

        lastComponentIds = currentIds;
        listContainer.Clear();

        // Since VerticalList will error out to 65532 if the list empty we add a dummy so the FitContents calculation works
        if (TargetList.Count == 0)
        {
            listContainer.AddNode(new ResNode { Height = 0, IsVisible = true });
        }


        foreach (var component in TargetList) {
            var node = new ComponentSettingsNode {
                Width = Width - 20.0f,
                HeaderHeight = 24.0f,
                OnChanged = () => System.OverlayManager.Setup(),
                OnDeleted = () => {
                    TargetList.Remove(component);
                    Refresh();
                    System.OverlayManager.Setup();
                },
                OnDuplicate = (source) => {
                    var copy = source.DeepCopy();

                    int index = TargetList.IndexOf(source);
                    if (index != -1 && index < TargetList.Count - 1)
                        TargetList.Insert(index + 1, copy);
                    else
                        TargetList.Add(copy);

                    Refresh();
                    System.OverlayManager.Setup();
                },
                OnToggle = RefreshLayout
            };

            node.RowComponent = component;

            listContainer.AddNode(node);
        }

        RefreshLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        addRow?.Width = Math.Max(0, Width - ContentNode.X);
    }
}
