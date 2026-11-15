using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.UiOverlay;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IAsyncDisposable, IDisposable {
    private bool isDisposed;
    private readonly object setupSync = new();
    private IFramework? setupFramework;
    private readonly Dictionary<string, MeterListLayoutNode> activeMeters = new();

    public async ValueTask DisposeAsync() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;
        CancelPendingSetup();

        await DetachAndDisposeAllAsync();
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;
        CancelPendingSetup();

        DetachAndDisposeAll();
    }

    public void Setup() {
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;

        try
        {
            Service.Framework.RunOnFrameworkThread(QueueSetupOnFrameworkThread);
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to schedule NativeMeters overlay setup.");
        }
    }

    private void QueueSetupOnFrameworkThread()
    {
        lock (setupSync)
        {
            if (isDisposed || Service.Framework.IsFrameworkUnloading || setupFramework != null) return;
            setupFramework = Service.Framework;
            // Rebuild after native UI callbacks return.
            setupFramework.Update += ApplyPendingSetup;
        }
    }

    private void ApplyPendingSetup(IFramework framework)
    {
        lock (setupSync)
        {
            if (!ReferenceEquals(setupFramework, framework)) return;
            framework.Update -= ApplyPendingSetup;
            setupFramework = null;
        }

        if (isDisposed || framework.IsFrameworkUnloading) return;
        SetupOnFrameworkThread();
    }

    private void CancelPendingSetup()
    {
        lock (setupSync)
        {
            if (setupFramework == null) return;
            setupFramework.Update -= ApplyPendingSetup;
            setupFramework = null;
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

