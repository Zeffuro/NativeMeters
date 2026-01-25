using System;
using System.Numerics;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : CategoryNode
{
    private ComponentSettings? settings;

    private readonly ComponentBasicsPanel basicsPanel;
    private readonly ComponentTypographyPanel typographyPanel;
    private readonly ComponentVisualsPanel visualsPanel;
    private readonly HorizontalListNode buttonRow;

    public Action? OnChanged { get; set; }
    public Action? OnDeleted { get; set; }
    public Action<ComponentSettings>? OnDuplicate { get; set; }

    public ComponentSettings? RowComponent
    {
        get => settings;
        set
        {
            settings = value;
            if (settings == null) return;

            String = $"{settings.Type} - {settings.Name}";

            basicsPanel.LoadSettings(settings);
            typographyPanel.LoadSettings(settings);
            visualsPanel.LoadSettings(settings);

            ContentNode.RecalculateLayout();
            RecalculateLayout();
        }
    }

    public ComponentSettingsNode()
    {
        AddTab();
        ItemSpacing = 0.0f;
        FirstItemSpacing = 6.0f;
        HeaderHeight = 24.0f;

        basicsPanel = new ComponentBasicsPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnNameChanged = name => { if (settings != null) String = $"{settings.Type} - {name}"; },
            OnLayoutChanged = RefreshLayout
        };

        typographyPanel = new ComponentTypographyPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnLayoutChanged = RefreshLayout
        };

        visualsPanel = new ComponentVisualsPanel
        {
            Width = Width,
            OnSettingsChanged = NotifyChanged,
            OnLayoutChanged = RefreshLayout
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

    private void NotifyChanged()
    {
        if (MeterDefinitionConfigurationNode.IsRefreshing) return;

        Util.SaveConfig(System.Config);
        OnChanged?.Invoke();
    }

    private void RefreshLayout()
    {
        ContentNode.RecalculateLayout();
        RecalculateLayout();
        OnToggle?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        if (basicsPanel == null || typographyPanel == null || visualsPanel == null || buttonRow == null) return;

        var innerWidth = Math.Max(0, Width - 30.0f);
        basicsPanel.Width = innerWidth;
        typographyPanel.Width = innerWidth;
        visualsPanel.Width = innerWidth;
    }
}
