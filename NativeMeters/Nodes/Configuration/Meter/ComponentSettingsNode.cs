using System;
using System.Numerics;
using KamiToolKit.BaseTypes;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Configuration.Meter.Panels;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : CollapsingHeaderNode, ILayoutListNode
{
    private const float HeaderHeight = 28.0f;
    private const float ChildIndent = 8.0f;

    private ComponentSettings? settings;

    private readonly ComponentBasicsPanel basicsPanel;
    private ComponentTypographyPanel? typographyPanel;
    private ComponentVisualsPanel? visualsPanel;
    private readonly HorizontalListNode buttonRow;

    private bool isLoadingComponent;
    private bool isCreatingPanels;
    private bool isInitialized;
    private float? lastLayoutWidth;

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
            String = settings == null ? string.Empty : $"{settings.Type} - {settings.Name}";

            isLoadingComponent = true;

            try
            {
                if (settings != null)
                {
                    basicsPanel.LoadSettings(settings);
                    typographyPanel?.LoadSettings(settings);
                    visualsPanel?.LoadSettings(settings);
                }
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

        basicsPanel = new ComponentBasicsPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnNameChanged = name => { if (settings != null) String = $"{settings.Type} - {name}"; },
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

        AddNode([basicsPanel, buttonRow]);
        isInitialized = true;
        RecalculateLayout();
    }

    public override void AddNode(NodeBase? node)
    {
        if (node != null) ApplyChildLayout(node);
        base.AddNode(node);
    }

    protected override void OnSizeChanged()
    {
        if (lastLayoutWidth == Width) return;
        lastLayoutWidth = Width;
        base.OnSizeChanged();

        foreach (var node in Nodes)
        {
            ApplyChildLayout(node);
        }
    }

    public new void RecalculateLayout()
    {
        OnRecalculateLayout();
        if (NavIndex != 0) OnRecalculateNavigation();
    }

    void ILayoutListNode.RecalculateLayout() => RecalculateLayout();

    protected override void OnRecalculateLayout()
    {
        if (!isInitialized || isCreatingPanels || isLoadingComponent) return;

        EnsureOptionalPanels();

        if (IsCollapsed || settings == null)
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
        if (settings == null || isLoadingComponent || isCreatingPanels || MeterDefinitionConfigurationNode.IsRefreshing) return;

        ConfigRepository.Save(System.Config);
        OnChanged?.Invoke();
    }

    private static void RecalculateChildLayout(NodeBase node)
    {
        if (node is not ILayoutListNode layoutNode) return;

        layoutNode.RecalculateLayout();
    }

    private void RefreshComponentLayout()
    {
        if (!isInitialized || isLoadingComponent || isCreatingPanels) return;

        RecalculateLayout();
        OnLayoutChanged?.Invoke();
    }

    private void ApplyChildLayout(NodeBase node)
    {
        node.X = ChildIndent;
        ConfigurationLayout.SetWidth(node, Math.Max(0.0f, Width - ChildIndent));
    }

    private bool ShouldShowNode(NodeBase node)
    {
        if (node == basicsPanel || node == buttonRow)
        {
            return true;
        }

        return node switch
        {
            _ when node == typographyPanel => settings?.Type == MeterComponentType.Text,
            _ when node == visualsPanel => settings?.Type is MeterComponentType.Text or MeterComponentType.ProgressBar or MeterComponentType.Background,
            _ => node.IsVisible
        };
    }

    private void EnsureOptionalPanels()
    {
        if (settings == null || IsCollapsed) return;
        var needsTypography = settings.Type == MeterComponentType.Text;
        var needsVisuals = settings.Type is MeterComponentType.Text or MeterComponentType.ProgressBar or MeterComponentType.Background;
        if ((!needsTypography || typographyPanel != null) && (!needsVisuals || visualsPanel != null)) return;

        isCreatingPanels = true;
        isLoadingComponent = true;
        try
        {
            if (needsTypography && typographyPanel == null)
            {
                typographyPanel = new ComponentTypographyPanel
                {
                    OnSettingsChanged = NotifyChanged,
                    OnLayoutChanged = RefreshComponentLayout
                };
                typographyPanel.LoadSettings(settings);
                AddNode(typographyPanel);
            }

            if (needsVisuals && visualsPanel == null)
            {
                visualsPanel = new ComponentVisualsPanel
                {
                    OnSettingsChanged = NotifyChanged,
                    OnLayoutChanged = RefreshComponentLayout
                };
                visualsPanel.LoadSettings(settings);
                AddNode(visualsPanel);
            }

            NodeList.Sort((left, right) => ChildOrder(left).CompareTo(ChildOrder(right)));
        }
        finally
        {
            isLoadingComponent = false;
            isCreatingPanels = false;
        }
    }

    private int ChildOrder(NodeBase node) => node == basicsPanel ? 0 : node == typographyPanel ? 1 : node == visualsPanel ? 2 : 3;

    private void SetButtonRowVisible(bool isVisible)
    {
        buttonRow.IsVisible = isVisible;

        foreach (var child in buttonRow.Nodes)
        {
            child.IsVisible = isVisible;
        }
    }
}
