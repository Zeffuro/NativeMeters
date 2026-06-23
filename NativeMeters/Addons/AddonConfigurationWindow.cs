using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Lumina.Data.Parsing.Uld;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Configuration.Connection;
using NativeMeters.Nodes.Configuration.General;
using NativeMeters.Nodes.Configuration.Meter;
using NativeMeters.Nodes.Configuration.Visibility;

namespace NativeMeters.Addons;

public class AddonConfigurationWindow : NativeAddon
{
    private readonly AddMeterDialogAddon addMeterDialog = new()
    {
        InternalName = "NativeMetersAddMeter",
        Title = "Add Meter",
        Size = new Vector2(440.0f, 208.0f),
        RememberClosePosition = false,
    };

    private TabBarNode tabBarNode = null!;

    private GeneralScrollingAreaNode generalScrollingAreaNode = null!;
    private ConnectionScrollingAreaNode connectionScrollingAreaNode = null!;
    private MeterManagementNode meterManagementNode = null!;
    private ColorConfigurationNode colorConfigurationNode = null!;
    private VisibilityScrollingAreaNode visibilityScrollingAreaNode = null!;

    private readonly List<NodeBase> tabContent = new();

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        var tabContentY = ContentStartPosition.Y + 40;
        var tabContentHeight = ContentSize.Y - 40;

        tabContent.Clear();

        tabBarNode = new TabBarNode
        {
            Position = ContentStartPosition,
            Size = ContentSize with { Y = 24 },
            IsVisible = true,
            NavIndex = 1,
            NavDown = 6,
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

        meterManagementNode = new MeterManagementNode(addMeterDialog)
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

        visibilityScrollingAreaNode = new VisibilityScrollingAreaNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = false,
        };
        visibilityScrollingAreaNode.AttachNode(this);

        tabContent.Add(generalScrollingAreaNode);
        tabContent.Add(connectionScrollingAreaNode);
        tabContent.Add(meterManagementNode);
        tabContent.Add(colorConfigurationNode);
        tabContent.Add(visibilityScrollingAreaNode);

        tabBarNode.AddTab(new TabBarEntry
        {
            TextId = 662, // General
            SheetType = NodeData.SheetType.Addon,
            OnClick = () => SwitchTab(0)
        });
        tabBarNode.AddTab("Connection", () => SwitchTab(1));
        tabBarNode.AddTab("Meters", () => SwitchTab(2));
        tabBarNode.AddTab("Colors", () => SwitchTab(3));
        tabBarNode.AddTab("Visibility", () => SwitchTab(4));

        base.OnSetup(addon, atkValueSpan);

        addon->UldManager.SetupTextRecursive();
        meterManagementNode.ApplyResolvedText();
    }

    private void SwitchTab(int index)
    {
        for (var i = 0; i < tabContent.Count; i++)
            tabContent[i].IsVisible = i == index;
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        System.Config.General.PreviewEnabled = false;
        addMeterDialog.OnMeterCreated = null;

        ConfigRepository.Save(System.Config);
        base.OnFinalize(addon);
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
    {
        addMeterDialog.Close();
        base.OnHide(addon);
    }

    public override void Dispose()
    {
        addMeterDialog.Close();
        addMeterDialog.OnMeterCreated = null;
        addMeterDialog.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await addMeterDialog.CloseAsync();
        addMeterDialog.OnMeterCreated = null;
        await addMeterDialog.DisposeAsync();
        await base.DisposeAsync();
    }
}
