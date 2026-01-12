using System;
using System.Collections.Generic;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IDisposable {
    private bool isDisposed;
    private readonly Dictionary<string, MeterListLayoutNode> activeMeters = new();

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
        foreach (var node in activeMeters.Values) {
            node.OnDispose();
            System.OverlayController.RemoveNode(node);
        }
        activeMeters.Clear();
    }

    private void CreateAndAttachOverlays()
    {
        foreach (var meterConfig in System.Config.Meters) {
            if (!meterConfig.IsEnabled || !System.Config.General.IsEnabled) continue;

            var node = new MeterListLayoutNode
            {
                MeterSettings = meterConfig
            };
            activeMeters.Add(meterConfig.Id, node);
            System.OverlayController.AddNode(node);
        }
    }

    public void UpdateActiveService()
    {
        IMeterService newService = System.Config.General.PreviewEnabled
            ? System.TestMeterService
            : System.MeterService;

        if (System.ActiveMeterService == newService) return;

        System.ActiveMeterService = newService;
        Setup();
    }
}

