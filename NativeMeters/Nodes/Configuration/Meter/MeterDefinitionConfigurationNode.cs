using System;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDefinitionConfigurationNode : SimpleComponentNode
{
    public Action? OnLayoutChanged { get; init; }

    private MeterSettings settings = new();

    private readonly ScrollingAreaNode<TreeListNode> scrollingArea;
    private readonly MeterGeneralSection generalSettings;
    private readonly MeterDisplaySection displaySettings;
    private readonly MeterComponentsSection headerSettings;
    private readonly MeterComponentsSection componentSettings;
    private readonly MeterComponentsSection footerSettings;

    public MeterDefinitionConfigurationNode()
    {
        scrollingArea = new ScrollingAreaNode<TreeListNode>
        {
            ContentHeight = 100.0f,
            AutoHideScrollBar = true,
        };
        scrollingArea.AttachNode(this);

        scrollingArea.ContentNode.OnLayoutUpdate = newHeight =>
        {
            scrollingArea.ContentHeight = newHeight;
        };

        scrollingArea.ContentNode.CategoryVerticalSpacing = 4.0f;

        var treeListNode = scrollingArea.ContentAreaNode;

        generalSettings = new MeterGeneralSection(() => settings)
        {
            String = "General Settings",
            IsCollapsed = false,
        };
        generalSettings.OnToggle = _ => HandleLayoutChange();
        treeListNode.AddCategoryNode(generalSettings);

        displaySettings = new MeterDisplaySection(() => settings)
        {
            String = "Display Settings",
            IsCollapsed = false,
        };
        displaySettings.OnToggle = _ => HandleLayoutChange();
        treeListNode.AddCategoryNode(displaySettings);

        headerSettings = new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Header)
        {
            String = "Header Components",
            IsCollapsed = true,
        };
        treeListNode.AddCategoryNode(headerSettings);

        componentSettings = new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Row)
        {
            String = "Row Components (Combatants)",
            IsCollapsed = true,
        };
        treeListNode.AddCategoryNode(componentSettings);

        footerSettings = new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Footer)
        {
            String = "Footer Components",
            IsCollapsed = true,
        };
        treeListNode.AddCategoryNode(footerSettings);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        scrollingArea.Size = Size;

        foreach (var categoryNode in scrollingArea.ContentNode.CategoryNodes)
        {
            categoryNode.Width = Width - 16.0f;
        }

        scrollingArea.ContentNode.RefreshLayout();
    }

    public void SetMeter(MeterSettings meterSettings)
    {
        settings = meterSettings;
        generalSettings.Refresh();
        displaySettings.Refresh();

        headerSettings.Refresh();
        componentSettings.Refresh();
        footerSettings.Refresh();

        HandleLayoutChange();
    }

    private void HandleLayoutChange()
    {
        scrollingArea.ContentNode.RefreshLayout();
        scrollingArea.ContentHeight = scrollingArea.ContentNode.Height;
        OnLayoutChanged?.Invoke();
    }
}