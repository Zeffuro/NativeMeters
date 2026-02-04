using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.ImportExport;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Configuration.Presets;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDefinitionConfigurationNode : SimpleComponentNode
{
    public Action? OnLayoutChanged { get; init; }

    private MeterSettings settings = new();

    private string? currentMeterId;
    public static bool IsRefreshing { get; private set; } = false;

    private readonly VerticalListNode containerLayout;
    private readonly SimpleComponentNode headerContainer;
    private readonly HorizontalListNode buttonsList;
    private readonly ScrollingAreaNode<VerticalListNode> scrollingArea;
    private readonly VerticalListNode mainLayout;

    private readonly List<MeterConfigSection> sections = [];

    public MeterDefinitionConfigurationNode()
    {
        containerLayout = new VerticalListNode
        {
            ItemSpacing = 4.0f,
            FitContents = false
        };
        containerLayout.AttachNode(this);

        headerContainer = new SimpleComponentNode
        {
            Height = 32,
        };
        containerLayout.AddNode(headerContainer);

        var titleLabel = new LabelTextNode
        {
            String = "Meter Configuration",
            FontSize = 18,
            TextColor = ColorHelper.GetColor(50),
            Size = new Vector2(200, 32),
            Position = new Vector2(5, 0)
        };
        titleLabel.AttachNode(headerContainer);

        buttonsList = new HorizontalListNode
        {
            Height = 32,
            ItemSpacing = 2.0f,
            Alignment = HorizontalListAnchor.Right
        };
        buttonsList.AttachNode(headerContainer);

        var presetsDropdown = new TextDropDownNode
        {
            Size = new Vector2(120, 28),
            Options = MeterPresets.GetPresetNames(),
            OnOptionSelected = HandlePresetSelection
        };
        buttonsList.AddNode(presetsDropdown);

        buttonsList.AddNode(new ImGuiIconButtonNode
        {
            Width = 28, Height = 28,
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\download.png"),
            TextTooltip = "Import Meter from Clipboard (Overwrites current)\n(hold shift to confirm)",
            OnClick = HandleImport
        });

        buttonsList.AddNode(new ImGuiIconButtonNode
        {
            Width = 28, Height = 28,
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\upload.png"),
            TextTooltip = "Export Meter to Clipboard",
            OnClick = () => ConfigPorter.TryExportMeterToClipboard(settings)
        });

        scrollingArea = new ScrollingAreaNode<VerticalListNode> {
            AutoHideScrollBar = true,
            ContentHeight = 10f,
        };
        containerLayout.AddNode(scrollingArea);

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

        containerLayout.Size = Size;

        headerContainer.Width = Width;

        float buttonsWidth = Math.Max(200, Width * 0.6f);
        buttonsList.Size = new Vector2(buttonsWidth, 32);
        buttonsList.Position = new Vector2(Width - buttonsWidth - 10, 2);
        buttonsList.RecalculateLayout();

        var scrollHeight = Math.Max(0, Height - headerContainer.Height - containerLayout.ItemSpacing);
        scrollingArea.Size = Size with { Y = scrollHeight };

        var listWidth = Math.Max(0, Width - 16.0f);
        mainLayout.Width = listWidth;

        foreach (var section in sections)
        {
            section.Width = listWidth;
        }

        HandleLayoutChange();
    }

    public void SetMeter(MeterSettings meterSettings)
    {
        if (currentMeterId == meterSettings.Id) return;
        currentMeterId = meterSettings.Id;

        settings = meterSettings;

        scrollingArea.IsVisible = false;
        IsRefreshing  = true;

        foreach (var section in sections)
        {
            section.Refresh();
        }

        IsRefreshing = false;
        scrollingArea.IsVisible = true;
        HandleLayoutChange();
    }

    private void RefreshAllSections()
    {
        scrollingArea.IsVisible = false;
        IsRefreshing = true;

        foreach (var section in sections)
        {
            section.Refresh();
        }

        IsRefreshing = false;
        scrollingArea.IsVisible = true;
        HandleLayoutChange();
    }

    private void HandleLayoutChange()
    {
        mainLayout.RecalculateLayout();

        scrollingArea.ContentHeight = mainLayout.Height;

        containerLayout.RecalculateLayout();

        OnLayoutChanged?.Invoke();
    }

    private void HandlePresetSelection(string presetName)
    {
        if (settings == null) return;

        MeterPresets.ApplyPreset(presetName, settings);

        ConfigRepository.Save(System.Config);
        System.OverlayManager.Setup();
        RefreshAllSections();
    }

    private void HandleImport()
    {
        if (!Service.KeyState[VirtualKey.SHIFT]) return;
        var newSettings = ConfigPorter.TryImportMeterFromClipboard();
        if (newSettings == null) return;

        MeterPresets.ApplySettings(newSettings, settings);

        ConfigRepository.Save(System.Config);
        System.OverlayManager.Setup();
        RefreshAllSections();
    }
}
