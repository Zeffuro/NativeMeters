using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Configuration.ImportExport;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Configuration.Presets;
using NativeMeters.Extensions;
using NativeMeters.Nodes.Configuration;
using NativeMeters.Nodes.Configuration.Meter.Sections;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Services;
using NativeMeters.Localization;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDefinitionConfigurationNode : ResNode
{
    private const float ScrollbarContentGutter = 14.0f;

    public Action? OnLayoutChanged { get; init; }
    public int NavigationStartIndex { get; set; } = 150;
    public int NavigationReturnIndex { get; set; } = 7;

    private MeterSettings settings = new();

    public static bool IsRefreshing { get; private set; } = false;

    private readonly VerticalListNode containerLayout;
    private readonly ResNode headerContainer;
    private readonly HorizontalListNode buttonsList;
    private readonly ScrollingNode<VerticalListNode> scrollingArea;
    private readonly MeterImportConfirmationAddon importConfirmation;
    private readonly Action<MeterSettings>? onNameChanged;

    private readonly List<MeterConfigSection> sections = [];
    private readonly Dictionary<MeterConfigSection, bool> sectionExpandedStates = [];

    internal MeterDefinitionConfigurationNode(MeterImportConfirmationAddon importConfirmation, Action<MeterSettings>? onNameChanged = null)
    {
        this.importConfirmation = importConfirmation;
        this.onNameChanged = onNameChanged;
        containerLayout = new VerticalListNode
        {
            ItemSpacing = 4.0f,
            FitContents = false
        };
        containerLayout.AttachNode(this);

        headerContainer = new ResNode
        {
            Height = 32,
        };
        containerLayout.AddNode(headerContainer);

        var titleLabel = new LabelTextNode
        {
            String = Loc.Get("MeterHeading"),
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

        var presetsDropdown = new StringDropDownNode
        {
            Size = new Vector2(250, 28),
            Options = MeterPresets.GetPresetNames(),
            OnOptionSelected = HandlePresetSelection
        };
        buttonsList.AddNode(presetsDropdown);

        buttonsList.AddNode(new ImGuiIconButtonNode
        {
            Width = 28, Height = 28,
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\download.png"),
            TextTooltip = Loc.Get("MeterImportTooltip"),
            OnClick = HandleImport
        });

        buttonsList.AddNode(new ImGuiIconButtonNode
        {
            Width = 28, Height = 28,
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\upload.png"),
            TextTooltip = Loc.Get("MeterExportTooltip"),
            OnClick = () => ConfigPorter.TryExportMeterToClipboard(settings)
        });

        scrollingArea = new ScrollingNode<VerticalListNode>
        {
            ContentNode =
            {
                FitContents = true,
                FitWidth = false,
                ItemSpacing = 6.0f,
            },
            AutoHideScrollBar = true,
            ScrollSpeed = 36,
            Size = Size
        };
        containerLayout.AddNode(scrollingArea);

        sections.Add(new MeterGeneralSection(() => settings, onNameChanged)
        {
            Width = 200,
            String = Loc.Get("MeterGeneralSettings"),
            IsCollapsed = false,
        });
        sections.Add(new MeterDisplaySection(() => settings)
        {
            String = Loc.Get("MeterDisplaySettings"),
            IsCollapsed = false,
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Header)
        {
            String = Loc.Get("MeterHeaderComponents"),
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Row)
        {
            String = Loc.Get("MeterRowComponents"),
        });
        sections.Add(new MeterComponentsSection(() => settings, HandleLayoutChange, ComponentTarget.Footer)
        {
            String = Loc.Get("MeterFooterComponents"),
        });

        foreach (var section in sections)
        {
            sectionExpandedStates[section] = !section.IsCollapsed;

            section.OnToggle = _ =>
            {
                var isExpanded = !section.IsCollapsed;
                if (sectionExpandedStates.TryGetValue(section, out var previousExpandedState)
                    && previousExpandedState == isExpanded)
                {
                    return;
                }

                sectionExpandedStates[section] = isExpanded;

                if (section is { IsCollapsed: false, IsInitialized: false })
                {
                    section.Refresh();
                    if (section is not MeterComponentsSection) HandleLayoutChange();
                }
                else
                {
                    HandleLayoutChange();
                }
            };

            scrollingArea.ContentNode.AddNode(section);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        ConfigurationLayout.SetSize(containerLayout, Size);

        ConfigurationLayout.SetWidth(headerContainer, Width);

        float buttonsWidth = Math.Max(200, Width * 0.6f);
        ConfigurationLayout.SetSize(buttonsList, new Vector2(buttonsWidth, 32));
        buttonsList.Position = new Vector2(Width - buttonsWidth - 10, 2);
        buttonsList.RecalculateLayout();

        var scrollHeight = Math.Max(0, Height - headerContainer.Height - containerLayout.ItemSpacing);
        ConfigurationLayout.SetSize(scrollingArea, new Vector2(Width, scrollHeight));

        HandleLayoutChange();
    }

    public void SetMeter(MeterSettings meterSettings)
    {
        if (ReferenceEquals(settings, meterSettings)) return;
        settings = meterSettings;

        IsRefreshing = true;
        try
        {
            foreach (var section in sections)
            {
                section.Refresh();
            }
        }
        finally
        {
            IsRefreshing = false;
        }
        HandleLayoutChange();
    }

    private void RefreshAllSections()
    {
        scrollingArea.IsVisible = false;
        IsRefreshing = true;
        try
        {
            foreach (var section in sections)
            {
                section.IsInitialized = false;
                section.Refresh();
            }
        }
        finally
        {
            IsRefreshing = false;
            scrollingArea.IsVisible = true;
        }
        HandleLayoutChange();
    }

    private void HandleLayoutChange()
    {
        if (IsRefreshing) return;
        RecalculateScrollingContent();
        ConfigurationNavigation.Apply(scrollingArea.ContentNode, NavigationStartIndex, NavigationReturnIndex, NavigationReturnIndex, NavigationReturnIndex, NavigationReturnIndex);
        OnLayoutChanged?.Invoke();
    }

    private void RecalculateScrollingContent()
    {
        using var layoutVisibility = new LayoutVisibilityScope(scrollingArea.ContentNode);
        var oldScrollPosition = scrollingArea.ScrollBarNode.ScrollPosition;
        var contentWidth = Math.Max(0.0f, scrollingArea.Width - ScrollbarContentGutter);

        ConfigurationLayout.SetWidth(scrollingArea.ContentNode, contentWidth);

        foreach (var section in sections)
        {
            ConfigurationLayout.SetWidth(section, contentWidth);
        }

        // The first pass lets nested sections settle after width/visibility changes;
        // the second pass positions siblings using those final section heights.
        scrollingArea.ContentNode.RecalculateLayout();
        scrollingArea.ContentNode.RecalculateLayout();
        scrollingArea.ScrollBarNode.UpdateScrollParams();
        scrollingArea.ScrollBarNode.ScrollPosition = Math.Clamp(oldScrollPosition, 0, scrollingArea.ScrollBarNode.ScrollMaxPosition);
    }

    private void HandlePresetSelection(string presetName)
    {
        if (settings == null) return;

        System.OverlayManager.CancelPlacement(settings);
        MeterPresets.ApplyPreset(presetName, settings);

        ConfigRepository.Save(System.Config);
        System.OverlayManager.Setup();
        RefreshAllSections();
    }

    private void HandleImport()
    {
        var target = settings;
        var config = System.Config;
        if (importConfirmation.IsBusy || !config.Meters.Contains(target)) return;

        var imported = ConfigPorter.ReadMeterFromClipboard();
        if (imported == null) return;

        importConfirmation.Request(target.Name, () =>
        {
            if (!ReferenceEquals(config, System.Config) || !ReferenceEquals(target, settings) || !config.Meters.Contains(target)) return;

            System.OverlayManager.CancelPlacement(target);
            var originalName = target.Name;
            MeterPresets.ApplySettings(imported, target);
            ConfigRepository.Save(config);
            System.OverlayManager.Setup();
            RefreshAllSections();
            if (target.Name != originalName) onNameChanged?.Invoke(target);
            Service.NotificationManager.Success(Loc.Get("NotifyMeterImported"));
        });
    }
}
