using System;
using System.Collections.Generic;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IDisposable {
    private bool isDisposed;
    private MeterListLayoutNode? meterListOverlay;
    private readonly Dictionary<string, MeterListLayoutNode> _activeMeters = new();

    public void Dispose() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;

        DetachAndDisposeAll();
    }

    public void Setup() {
        DetachAndDisposeAll();

        Service.Framework.RunOnFrameworkThread(CreateAndAttachOverlays);
    }

    private void DetachAndDisposeAll() {
        foreach (var node in _activeMeters.Values) {
            node.OnDispose();
            System.OverlayController.RemoveNode(node);
        }
        _activeMeters.Clear();
    }

    private void CreateAndAttachOverlays()
    {
        foreach (var meterConfig in System.Config.Meters) {
            if (!meterConfig.IsEnabled) continue;

            var node = new MeterListLayoutNode
            {
                MeterSettings = meterConfig
            };
            _activeMeters.Add(meterConfig.Id, node);
            System.OverlayController.AddNode(node);
        }
    }
}

