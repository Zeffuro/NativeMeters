using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.UiOverlay;
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
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;

        try
        {
            Service.Framework.RunOnFrameworkThread(SetupOnFrameworkThread);
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to schedule NativeMeters overlay setup.");
        }
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
        await Service.Framework.RunOnFrameworkThreadIfNeeded(DetachAndDisposeAllOnFrameworkThread);
    }

    private void DetachAndDisposeAllOnFrameworkThread()
    {
        foreach (var node in activeMeters.Values)
        {
            DetachAndDisposeOverlayNode(node, unregisterFromController: !isDisposed);
        }

        activeMeters.Clear();
    }

    private static void DetachAndDisposeOverlayNode(MeterListLayoutNode node, bool unregisterFromController)
    {
        node.OnDispose();
        node.IsVisible = false;
        node.EnableMoving = false;
        node.EnableResizing = false;
        node.RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision, NodeFlags.Focusable);

        node.DetachNode();
        node.Dispose();

        if (unregisterFromController)
            System.OverlayController?.RemoveNode(node);
    }

    private void SetupOnFrameworkThread()
    {
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;

        try
        {
            System.OverlayController ??= new OverlayController();

            DetachAndDisposeAllOnFrameworkThread();
            CreateAndAttachOverlays();
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to setup NativeMeters overlays.");
        }
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
            System.OverlayController?.AddNode(node);
        }
    }

    public void UpdateSettings()
    {
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;

        foreach (var node in activeMeters.Values)
        {
            node.UpdateSettings();
        }
    }

    public void UpdateActiveService()
    {
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;

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

