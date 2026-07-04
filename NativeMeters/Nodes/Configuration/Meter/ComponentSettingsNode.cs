using System;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Configuration.Meter.Panels;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : CollapsingHeaderNode
{
    private const float HeaderHeight = 28.0f;
    private const float ChildIndent = 8.0f;

    private ComponentSettings? settings;

    private readonly ComponentBasicsPanel basicsPanel;
    private readonly ComponentTypographyPanel typographyPanel;
    private readonly ComponentVisualsPanel visualsPanel;
    private readonly HorizontalListNode buttonRow;

    private bool isLoadingComponent;

    public Action? OnChanged { get; set; }
    public Action? OnDeleted { get; set; }
    public Action? OnLayoutChanged { get; set; }
    public Action<ComponentSettings>? OnDuplicate { get; set; }

    public ComponentSettings? RowComponent
    {
        get => settings;
        set
        {
            settings = value;
            if (settings == null) return;

            String = $"{settings.Type} - {settings.Name}";

            isLoadingComponent = true;

            try
            {
                basicsPanel.LoadSettings(settings);
                typographyPanel.LoadSettings(settings);
                visualsPanel.LoadSettings(settings);
            }
            finally
            {
                isLoadingComponent = false;
            }

            RefreshComponentLayout();
        }
    }

    public ComponentSettingsNode()
    {
        ItemSpacing = 0.0f;
        FirstItemSpacing = 6.0f;
        FitWidth = false;
        ReverseLayoutUpdate = true;

        basicsPanel = new ComponentBasicsPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnNameChanged = name => { if (settings != null) String = $"{settings.Type} - {name}"; },
            OnLayoutChanged = RefreshComponentLayout
        };

        typographyPanel = new ComponentTypographyPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnLayoutChanged = RefreshComponentLayout
        };

        visualsPanel = new ComponentVisualsPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnLayoutChanged = RefreshComponentLayout
        };

        buttonRow = new HorizontalListNode {
            Size = new Vector2(Width, 24),
            ItemSpacing = 4.0f
        };

        var deleteButton = new TextButtonNode()
        {
            String = "Delete",
            Size = new Vector2(100, 24),
            OnClick = () => OnDeleted?.Invoke()
        };

        var duplicateButton = new TextButtonNode()
        {
            String = "Duplicate",
            Size = new Vector2(100, 24),
            OnClick = () =>
            {
                if(settings != null) OnDuplicate?.Invoke(settings);
            }
        };

        buttonRow.AddNode([duplicateButton, deleteButton]);

        AddNode([basicsPanel, typographyPanel, visualsPanel, buttonRow]);
    }

    public override void AddNode(NodeBase? node)
    {
        if (node != null) ApplyChildLayout(node);
        base.AddNode(node);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        foreach (var node in Nodes)
        {
            ApplyChildLayout(node);
        }
    }

    protected override void OnRecalculateLayout()
    {
        if (IsCollapsed)
        {
            foreach (var node in Nodes)
            {
                node.IsVisible = false;
                if (node == buttonRow) SetButtonRowVisible(false);
            }

            Height = HeaderHeight;
            return;
        }

        var yPosition = HeaderHeight + FirstItemSpacing;

        foreach (var node in Nodes)
        {
            node.IsVisible = ShouldShowNode(node);
            if (node == buttonRow) SetButtonRowVisible(node.IsVisible);
            if (!node.IsVisible) continue;

            RecalculateChildLayout(node);
            node.Y = yPosition;
            yPosition += node.Height + ItemSpacing;
        }

        Height = yPosition;
    }

    private void NotifyChanged()
    {
        if (MeterDefinitionConfigurationNode.IsRefreshing) return;

        ConfigRepository.Save(System.Config);
        OnChanged?.Invoke();
    }

    private static void RecalculateChildLayout(NodeBase node)
    {
        if (node is not ILayoutListNode layoutNode) return;

        layoutNode.RecalculateLayout(true);
    }

    private void RefreshComponentLayout()
    {
        if (isLoadingComponent) return;

        RecalculateLayout();
        OnLayoutChanged?.Invoke();
    }

    private void ApplyChildLayout(NodeBase node)
    {
        node.X = ChildIndent;
        node.Width = Math.Max(0.0f, Width - ChildIndent);
    }

    private bool ShouldShowNode(NodeBase node)
    {
        if (node == basicsPanel || node == buttonRow)
        {
            return true;
        }

        if (settings == null)
        {
            return node.IsVisible;
        }

        return node switch
        {
            _ when node == typographyPanel => settings.Type == MeterComponentType.Text,
            _ when node == visualsPanel => settings.Type is MeterComponentType.Text or MeterComponentType.ProgressBar or MeterComponentType.Background,
            _ => node.IsVisible
        };
    }

    private void SetButtonRowVisible(bool isVisible)
    {
        buttonRow.IsVisible = isVisible;

        foreach (var child in buttonRow.Nodes)
        {
            child.IsVisible = isVisible;
        }
    }
}
