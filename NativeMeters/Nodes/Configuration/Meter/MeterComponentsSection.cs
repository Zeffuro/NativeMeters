using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterComponentsSection : MeterConfigSection
{
    private readonly Action onLayoutChanged;

    public MeterComponentsSection(Func<MeterSettings> getSettings, Action onLayoutChanged) : base(getSettings)
    {
        this.onLayoutChanged = onLayoutChanged;
    }

    private void AddNewComponent(string? selectedComponent)
    {
        if (!Enum.TryParse<MeterComponentType>(selectedComponent, out var parsedComponentType))
            return;

        var component = new RowComponentSettings {
            Type = parsedComponentType,
            Name = $"New {parsedComponentType}",
            Size = new Vector2(100, 20)
        };
        Settings.RowComponents.Add(component);
        Refresh();
        System.OverlayManager.Setup();
    }

    public override void Refresh()
    {
        // 1. Clear all child nodes from the TreeListCategoryNode
        foreach (var child in Children)
        {
            child.Dispose();
        }

        // 2. Add Component Nodes
        foreach (var component in Settings.RowComponents)
        {
            var node = new ComponentSettingsNode(
                component,
                () => System.OverlayManager.Setup(),
                () => {
                    Settings.RowComponents.Remove(component);
                    Refresh();
                    System.OverlayManager.Setup();
                },
                () => {
                    // When a child toggles, recalculate this category, then the scroll area
                    this.RecalculateLayout();
                    onLayoutChanged();
                }
            ) {
                Width = Width - 10.0f
            };
            AddNode(node);
        }

        // 3. Add the "Add Component" Row at the bottom
        var addRow = new HorizontalListNode { Size = new Vector2(Width, 32), ItemSpacing = 5.0f, Position = new Vector2(0, 4) };
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

        // 4. Final layout update
        RecalculateLayout();
        onLayoutChanged();
    }
}