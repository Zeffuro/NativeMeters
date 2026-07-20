using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IAsyncDisposable, IDisposable {
    private bool isDisposed;
    private readonly Dictionary<string, MeterListLayoutNode> activeMeters = new();

    public async ValueTask DisposeAsync() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;

        await DetachAndDisposeAllAsync();
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;

        DetachAndDisposeAll();
    }

    public void Setup() {
        Service.Framework.RunOnFrameworkThread(() => {
            DetachAndDisposeAllOnFrameworkThread();
            CreateAndAttachOverlays();
        });
    }

    private void DetachAndDisposeAll()
    {
        if (Service.Framework.IsFrameworkUnloading)
        {
            activeMeters.Clear();
            return;
        }

        Service.Framework.RunOnFrameworkThread(() =>
        {
            DetachAndDisposeAllOnFrameworkThread();
        });
    }

    private async ValueTask DetachAndDisposeAllAsync()
    {
        if (Service.Framework.IsFrameworkUnloading)
        {
            activeMeters.Clear();
            return;
        }

        await Service.Framework.RunSafely(DetachAndDisposeAllOnFrameworkThread);
    }

    private void DetachAndDisposeAllOnFrameworkThread()
    {
        foreach (var node in activeMeters.Values)
        {
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

    public void UpdateSettings()
    {
        foreach (var node in activeMeters.Values)
        {
            node.UpdateSettings();
        }
    }

    public void UpdateActiveService()
    {
        IMeterService newService;

        if (System.Config.General.PreviewEnabled)
        {
            newService = System.TestMeterService;
        }
        else if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
        {
            newService = System.InternalMeterService;
        }
        else
        {
            newService = System.MeterService;
        }

        if (System.ActiveMeterService == newService) return;

        System.ActiveMeterService = newService;
        foreach (var meter in activeMeters)
        {
            meter.Value.SubscribeToCombatDataUpdates();
        }
    }
}

