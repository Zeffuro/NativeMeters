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

        // Since VerticalList will error out to 65532 if the list empty we add a dummy so the FitContents calculation works
        if (TargetList.Count == 0)
        {
            listContainer.AddNode(new ResNode { Height = 0, IsVisible = true });
        }

        foreach (var component in TargetList) {
            var node = new ComponentSettingsNode {
                Width = Width - 20.0f,
                HeaderHeight = 24.0f,
                RowComponent = component,
                OnChanged = () => System.OverlayManager.Setup(),
                OnDeleted = () => {
                    TargetList.Remove(component);
                    Refresh();
                    System.OverlayManager.Setup();
                },
                OnToggle = RefreshLayout
            };

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
