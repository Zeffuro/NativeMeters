using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

    public MeterComponentsSection(Func<MeterSettings> getSettings, Action onLayoutChanged, ComponentTarget target) : base(getSettings)
    {
        AddTab();
        this.onLayoutChanged = onLayoutChanged;
        this.target = target;

        listContainer = new VerticalListNode {
            ItemSpacing = 4.0f,
            FitContents = true,
        };
        AddNode(listContainer);

        addRow = new HorizontalListNode
        {
            Size = new Vector2(300, 30),
            ItemSpacing = 5.0f,
            Position = new Vector2(0, 10)
        };
        AddNode(addRow);

        InitializeAddRow();
    }

    private List<ComponentSettings> TargetList => target switch
    {
        ComponentTarget.Header => Settings.HeaderComponents,
        ComponentTarget.Row => Settings.RowComponents,
        ComponentTarget.Footer => Settings.FooterComponents,
        _ => Settings.RowComponents
    };

    private void InitializeAddRow()
    {
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
    }

    private void AddNewComponent(string? selectedComponent)
    {
        if (!Enum.TryParse<MeterComponentType>(selectedComponent, out var parsedComponentType)) return;

        var component = new ComponentSettings {
            Type = parsedComponentType,
            Name = $"New {parsedComponentType}",
            Size = new Vector2(100, 20)
        };

        TargetList.Add(component);
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
        listContainer.Clear();

        foreach (var component in TargetList) {
            var node = new ComponentSettingsNode {
                Position = new Vector2(10.0f, 0),
                Width = Width - 20.0f,
                HeaderHeight = 24.0f,
                RowComponent = component,
                OnChanged = () => System.OverlayManager.Setup(),
                OnDeleted = () => {
                    TargetList.Remove(component);
                    Refresh();
                    System. OverlayManager.Setup();
                },
                OnToggle = RefreshLayout
            };

            listContainer.AddNode(node);
        }

        RefreshLayout();
    }
}
