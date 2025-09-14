using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Nodes;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public unsafe class OverlayController : IDisposable
{
    private bool isDisposed = false;
    private OverlayRootNode? nativeMetersRootNode;
    private MeterListLayoutNode? meterListLayoutNode;

    public OverlayController() {
        Service.NameplateAddonController.OnAttach += AttachNodes;
        Service.NameplateAddonController.OnDetach += DetachNodes;
    }

    private void AttachNodes(AddonNamePlate* addonNamePlate) {
        DetachAndDisposeAll();

        var screenSize = new Vector2(AtkStage.Instance()->ScreenSize.Width, AtkStage.Instance()->ScreenSize.Height);

        nativeMetersRootNode = new OverlayRootNode(screenSize, Service.NativeController);

        meterListLayoutNode = new MeterListLayoutNode {
            NodeId = 2,
            X = 1900,
            Y = 1100,
            Width = 300,
            Height = 200,
            IsVisible = true,
        };

        nativeMetersRootNode.AddOverlay(meterListLayoutNode);
        //_nativeMetersRootNode.AddOverlay(_playerCombinedOverlay);

        /*
        _colorPickerAddon = new ColorPickerAddon(this) {
            InternalName = "StatusTimerColorPicker",
            Title = "Pick a color",
            Size = new Vector2(540, 500),
            NativeController = Services.Services.NativeController
        };

        _configurationWindow = new ConfigurationWindow(this) {
            InternalName = "StatusTimersConfiguration",
            Title = "StatusTimers Configuration",
            Size = new Vector2(640, 512),
            NativeController = Services.Services.NativeController
        };
        */

        if (addonNamePlate != null && nativeMetersRootNode != null)
        {
            nativeMetersRootNode.AttachAllToNativeController(addonNamePlate->RootNode);
        }

        //_enemyMultiDoTOverlay?.Setup();
    }

    public void OnUpdate() {
        if (isDisposed) {
            return;
        }

        //_playerCombinedOverlay?.OnUpdate();
    }

    private void DetachNodes(AddonNamePlate* addonNamePlate) {
        DetachAndDisposeAll();
    }

    private void DetachAndDisposeAll() {
        /*
        if (_colorPickerAddon != null)
        {
            _colorPickerAddon.Dispose();
            _colorPickerAddon = null;
        }
        if (_configurationWindow != null)
        {
            _configurationWindow.Dispose();
            _configurationWindow = null;
        }
        */
        if (nativeMetersRootNode != null)
        {
            nativeMetersRootNode.Cleanup();
            nativeMetersRootNode = null;
        }
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;

        DetachAndDisposeAll();
    }
}