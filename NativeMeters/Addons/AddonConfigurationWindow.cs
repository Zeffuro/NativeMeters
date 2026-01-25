using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Configuration.Connection;
using NativeMeters.Nodes.Configuration.General;
using NativeMeters.Nodes.Configuration.Meter;

namespace NativeMeters.Addons;

public class AddonConfigurationWindow : NativeAddon
{
    private TabBarNode tabBarNode = null!;

    private GeneralScrollingAreaNode generalScrollingAreaNode = null!;
    private ConnectionScrollingAreaNode connectionScrollingAreaNode = null!;
    private MeterManagementNode meterManagementNode = null!;
    private ColorConfigurationNode colorConfigurationNode = null!;

    private readonly List<NodeBase> tabContent = new();

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        var tabContentY = ContentStartPosition.Y + 40;
        var tabContentHeight = ContentSize.Y - 40;

        tabContent.Clear();

        tabBarNode = new TabBarNode
        {
            Position = ContentStartPosition,
            Size = ContentSize with { Y = 24 },
            IsVisible = true
        };
        tabBarNode.AttachNode(this);

        generalScrollingAreaNode = new GeneralScrollingAreaNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = true,
        };
        generalScrollingAreaNode.AttachNode(this);

        connectionScrollingAreaNode = new ConnectionScrollingAreaNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = false,
        };
        connectionScrollingAreaNode.AttachNode(this);

        meterManagementNode = new MeterManagementNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = false,
        };
        meterManagementNode.AttachNode(this);

        colorConfigurationNode = new ColorConfigurationNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = false,
        };
        colorConfigurationNode.AttachNode(this);

        tabContent.Add(generalScrollingAreaNode);
        tabContent.Add(connectionScrollingAreaNode);
        tabContent.Add(meterManagementNode);
        tabContent.Add(colorConfigurationNode);

        tabBarNode.AddTab("General", () => SwitchTab(0));
        tabBarNode.AddTab("Connection", () => SwitchTab(1));
        tabBarNode.AddTab("Meters", () => SwitchTab(2));
        tabBarNode.AddTab("Colors", () => SwitchTab(3));

        base.OnSetup(addon);
    }

    private void SwitchTab(int index)
    {
        for (var i = 0; i < tabContent.Count; i++)
            tabContent[i].IsVisible = i == index;
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        System.Config.General.PreviewEnabled = false;

        ConfigRepository.Save(System.Config);
        base.OnFinalize(addon);
    }
}
