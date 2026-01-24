using System;
using System.Collections.Generic;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDefinitionConfigurationNode : SimpleComponentNode
{
    public Action? OnLayoutChanged { get; init; }

    private MeterSettings settings = new();

    private string? currentMeterId;

    private readonly ScrollingAreaNode<VerticalListNode> scrollingArea;
    private readonly VerticalListNode mainLayout;

    private readonly List<MeterConfigSection> sections = [];

    public MeterDefinitionConfigurationNode()
    {
        scrollingArea = new ScrollingAreaNode<VerticalListNode> {
            AutoHideScrollBar = true,
            ContentHeight = 10f,
        };
        scrollingArea.AttachNode(this);

        mainLayout = scrollingArea.ContentAreaNode;
        mainLayout.FitContents = true;
        mainLayout.ItemSpacing = 6.0f;

        sections.Add(new MeterGeneralSection(() => settings)
        {
            String = "General Settings", IsCollapsed = false,
        });
        sections.Add(new MeterDisplaySection(() => settings)
        {
            String = "Display Settings", IsCollapsed = false,
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Header)
        {
            String = "Header Components",
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Row)
        {
            String = "Row Components (Combatants)",
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Footer)
        {
            String = "Footer Components",
        });

        foreach (var section in sections)
        {
            section.OnToggle = HandleLayoutChange;
            mainLayout.AddNode(section);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        scrollingArea.Size = Size;

        var listWidth = Math.Max(0, Width - 16.0f);
        mainLayout.Width = listWidth;

        foreach (var section in sections)
            section.Width = listWidth;

        HandleLayoutChange();
    }

    public void SetMeter(MeterSettings meterSettings)
    {
        if (currentMeterId == meterSettings.Id) return;
        currentMeterId = meterSettings.Id;

        settings = meterSettings;

        scrollingArea.IsVisible = false;

        foreach (var section in sections)
        {
            section.Refresh();
        }

        scrollingArea.IsVisible = true;
        HandleLayoutChange();
    }

    private void HandleLayoutChange()
    {
        mainLayout.RecalculateLayout();

        scrollingArea.ContentHeight = mainLayout.Height;

        OnLayoutChanged?.Invoke();
    }
}
