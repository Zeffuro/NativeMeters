using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.Premade.Addons;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IDisposable {
    private bool _isDisposed;
    private MeterListLayoutNode? _meterListOverlay;

    public void Dispose() {
        if (_isDisposed) {
            return;
        }
        _isDisposed = true;

        DetachAndDisposeAll();
    }

    public void Setup() {
        DetachAndDisposeAll();

        Service.Framework.RunOnFrameworkThread(CreateAndAttachOverlays);
    }

    private void DetachAndDisposeAll() {
        _meterListOverlay?.OnDispose(true);
        _meterListOverlay = null;
    }

    private void CreateAndAttachOverlays()
    {
        _meterListOverlay = new MeterListLayoutNode();
        System.OverlayController.AddNode(_meterListOverlay);
    }
}

