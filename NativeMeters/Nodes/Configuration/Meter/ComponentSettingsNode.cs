using System;
using System.Numerics;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : CategoryNode
{
    private ComponentSettings? settings;

    private readonly ComponentBasicsPanel basicsPanel;
    private readonly ComponentTypographyPanel typographyPanel;
    private readonly ComponentVisualsPanel visualsPanel;
    private readonly TextButtonNode deleteBtn;

    public Action? OnChanged { get; set; }
    public Action? OnDeleted { get; set; }

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

        deleteBtn = new TextButtonNode
        {
            String = "Delete Component",
            Size = new Vector2(Width, 24),
            Color = ColorHelper.GetColor(17),
            OnClick = () => OnDeleted?.Invoke()
        };

        AddNode([basicsPanel, typographyPanel, visualsPanel, deleteBtn]);
    }

    private void NotifyChanged() => OnChanged?.Invoke();

    private void RefreshLayout()
    {
        ContentNode.RecalculateLayout();
        RecalculateLayout();
        OnToggle?.Invoke();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (deleteBtn == null) return;

        var innerWidth = Math.Max(0, Width - 30.0f);
        basicsPanel.Width = innerWidth;
        typographyPanel.Width = innerWidth;
        visualsPanel.Width = innerWidth;
        deleteBtn.Width = innerWidth;
    }
}
