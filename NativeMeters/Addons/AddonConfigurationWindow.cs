using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Localization;
using NativeMeters.Nodes.Configuration.Connection;
using NativeMeters.Nodes.Configuration.General;
using NativeMeters.Nodes.Configuration.Meter;
using NativeMeters.Nodes.Configuration.PartyList;
using NativeMeters.Nodes.Configuration.Visibility;
using NativeMeters.Services;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Addons;

public class AddonConfigurationWindow : NativeAddon
{
    private readonly MeterImportConfirmationAddon meterImportConfirmation;
    private readonly AddMeterDialogAddon addMeterDialog = new()
    {
        InternalName = "NativeMetersAddMeter",
        Title = Loc.GetWindowHeader("MeterAdd").Title,
        Size = new Vector2(DialogWidth, DialogHeight),
        RememberClosePosition = false,
    };

    private TabBarNode tabBarNode = null!;

    private GeneralScrollingAreaNode generalScrollingAreaNode = null!;
    private ConnectionScrollingAreaNode connectionScrollingAreaNode = null!;
    private PartyListScrollingAreaNode partyListScrollingAreaNode = null!;
    private MeterManagementNode meterManagementNode = null!;
    private ColorConfigurationNode colorConfigurationNode = null!;
    private VisibilityScrollingAreaNode visibilityScrollingAreaNode = null!;

    private readonly List<NodeBase> tabContent = new();
    private volatile bool isDisposing;
    private bool reopenForLanguageChange;
    private float languageChangeScrollPosition;

    public AddonConfigurationWindow()
    {
        meterImportConfirmation = new MeterImportConfirmationAddon(this)
        {
            InternalName = "NativeMetersImportConfirm",
            Title = Loc.GetWindowHeader("WindowImportMeter").Title,
            Size = new Vector2(456, 134),
            RememberClosePosition = false,
            EnableContextMenu = false,
            DepthLayer = 6,
        };
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        var isLanguageReopen = reopenForLanguageChange;
        reopenForLanguageChange = false;
        if (!isLanguageReopen) System.OverlayManager?.CancelPlacement();

        this.SetLocalizedTitle("WindowConfiguration");
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

        generalScrollingAreaNode = new GeneralScrollingAreaNode(ChangeLanguage)
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

        partyListScrollingAreaNode = new PartyListScrollingAreaNode
        {
            Position = ContentStartPosition with { Y = tabContentY },
            Size = ContentSize with { Y = tabContentHeight },
            IsVisible = false,
        };
        partyListScrollingAreaNode.AttachNode(this);

        meterManagementNode = new MeterManagementNode(addMeterDialog, meterImportConfirmation)
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
        tabContent.Add(partyListScrollingAreaNode);
        tabContent.Add(meterManagementNode);
        tabContent.Add(colorConfigurationNode);
        tabContent.Add(visibilityScrollingAreaNode);

        tabBarNode.AddTab(Loc.Get("TabGeneral"), () => SwitchTab(0));
        tabBarNode.AddTab(Loc.Get("TabConnection"), () => SwitchTab(1));
        tabBarNode.AddTab(Loc.Get("TabPartyList"), () => SwitchTab(2));
        tabBarNode.AddTab(Loc.Get("TabMeters"), () => SwitchTab(3));
        tabBarNode.AddTab(Loc.Get("TabColors"), () => SwitchTab(4));
        tabBarNode.AddTab(Loc.Get("TabVisibility"), () => SwitchTab(5));

        base.OnSetup(addon, atkValueSpan);

        if (isLanguageReopen)
        {
            var scrollBar = generalScrollingAreaNode.ScrollBarNode;
            scrollBar.ScrollPosition = Math.Clamp(languageChangeScrollPosition, 0, scrollBar.ScrollMaxPosition);
        }

        addon->UldManager.SetupTextRecursive();
    }

    private void ChangeLanguage(string? language)
    {
        // The dropdown still uses its native nodes after invoking this callback.
        _ = Service.Framework.RunOnTick(() =>
        {
            if (!CanReopen() || Loc.LanguageOverride == language) return;

            Loc.SetLanguage(language);
            System.Config.General.UiLanguage = Loc.LanguageOverride;
            ConfigRepository.Save(System.Config);

            if (!IsOpen || reopenForLanguageChange) return;

            languageChangeScrollPosition = generalScrollingAreaNode.ScrollBarNode.ScrollPosition;
            reopenForLanguageChange = true;
            Close();
        }, delayTicks: 1);
    }

    private bool CanReopen()
        => !isDisposing && ReferenceEquals(System.AddonConfigurationWindow, this)
            && Service.Framework is { IsFrameworkUnloading: false };

    private unsafe void ReopenAfterLanguageChange(int ticksRemaining)
    {
        if (!reopenForLanguageChange || !CanReopen()) return;

        if (InternalAddon is null)
        {
            Open();
            if (InternalAddon is null) reopenForLanguageChange = false;
        }
        else if (ticksRemaining > 0)
        {
            _ = Service.Framework.RunOnTick(() => ReopenAfterLanguageChange(ticksRemaining - 1), delayTicks: 1);
        }
        else
        {
            reopenForLanguageChange = false;
            Service.Logger.Warning("The settings window did not finish closing after the language changed.");
        }
    }

    internal void CancelLanguageReopen() => isDisposing = true;

    private void SwitchTab(int index)
    {
        if (index != 3) meterImportConfirmation.Cancel();
        for (var i = 0; i < tabContent.Count; i++)
        {
            var isSelected = i == index;
            tabContent[i].IsVisible = isSelected;

            if (isSelected)
            {
                RecalculateTabContent(i);
            }
        }
    }

    private void RecalculateTabContent(int index)
    {
        switch (index)
        {
            case 0:
                generalScrollingAreaNode.RecalculateSizes();
                break;
            case 1:
                connectionScrollingAreaNode.RecalculateSizes();
                break;
            case 2:
                partyListScrollingAreaNode.RecalculateSizes();
                break;
            case 3:
                meterManagementNode.RecalculateLayout();
                break;
            case 4:
                colorConfigurationNode.RecalculateSizes();
                break;
            case 5:
                visibilityScrollingAreaNode.RecalculateSizes();
                break;
        }
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        meterImportConfirmation.Cancel();
        addMeterDialog.OnMeterCreated = null;

        if (reopenForLanguageChange && CanReopen())
        {
            base.OnFinalize(addon);
            _ = Service.Framework.RunOnTick(() => ReopenAfterLanguageChange(60), delayTicks: 1);
            return;
        }

        var config = System.Config;
        if (config is not null)
        {
            config.General.PreviewEnabled = false;
        }

        if (Service.Framework is not null)
        {
            System.OverlayManager?.UpdateActiveService();
            System.PartyListMeterManager?.UpdateSettings();
        }

        if (config is not null)
        {
            ConfigRepository.Save(config);
        }

        base.OnFinalize(addon);
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
    {
        meterImportConfirmation.Cancel();
        addMeterDialog.Close();
        System.TextFlagsPickerAddon?.Close();
        base.OnHide(addon);
    }

    public override void Dispose()
    {
        CancelLanguageReopen();
        meterImportConfirmation.Dispose();
        addMeterDialog.Close();
        System.TextFlagsPickerAddon?.Close();
        addMeterDialog.OnMeterCreated = null;
        addMeterDialog.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        CancelLanguageReopen();
        if (ThreadSafety.IsMainThread)
        {
            Dispose();
            return;
        }

        await addMeterDialog.CloseAsync();
        await meterImportConfirmation.DisposeAsync();
        if (System.TextFlagsPickerAddon is not null)
        {
            await System.TextFlagsPickerAddon.CloseAsync();
        }

        addMeterDialog.OnMeterCreated = null;
        await addMeterDialog.DisposeAsync();
        await base.DisposeAsync();
    }
}
