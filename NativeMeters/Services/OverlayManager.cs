using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.UiOverlay;
using NativeMeters.Addons;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Services;

public class OverlayManager : IAsyncDisposable, IDisposable {
    private bool isDisposed;
    private readonly object setupSync = new();
    private IFramework? setupFramework;
    private readonly Dictionary<string, MeterListLayoutNode> activeMeters = new();
    private MeterPlacementSession? placement;

    public async ValueTask DisposeAsync() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;
        CancelPendingSetup();
        placement?.Cancel();

        await DetachAndDisposeAllAsync();
    }

    public void Dispose() {
        if (isDisposed) {
            return;
        }
        isDisposed = true;
        CancelPendingSetup();
        placement?.Cancel();

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
        foreach (var meterConfig in System.Config.Meters)
            CreateAndAttachOverlay(meterConfig);
    }

    private void CreateAndAttachOverlay(MeterSettings meter)
    {
        if (!meter.IsEnabled || !System.Config.General.IsEnabled) return;

        var node = new MeterListLayoutNode { MeterSettings = meter, Placement = GetPlacement(meter) };
        activeMeters.Add(meter.Id, node);
        System.OverlayController?.AddNode(node);
    }

    private bool IsCurrentMeter(MeterListLayoutNode node, MeterSettings meter)
        => !isDisposed && !Service.Framework.IsFrameworkUnloading
            && activeMeters.TryGetValue(meter.Id, out var current) && ReferenceEquals(current, node);

    private void RefreshMeter(MeterListLayoutNode node, MeterSettings meter)
    {
        if (!IsCurrentMeter(node, meter)) return;
        activeMeters.Remove(meter.Id);
        DetachAndDisposeOverlayNode(node, unregisterFromController: true);
        if (System.Config.Meters.Contains(meter))
            CreateAndAttachOverlay(meter);
    }

    internal void QueueMeterRefresh(MeterListLayoutNode node)
    {
        if (node.MeterSettings is { } meter)
            _ = Service.Framework.RunOnTick(() => RefreshMeter(node, meter), delayTicks: 1);
    }

    internal void RequestMeterLock(MeterListLayoutNode node)
    {
        if (node.Placement is { } session)
        {
            session.RequestOutcome(MeterPlacementOutcome.Lock);
            return;
        }

        if (node.MeterSettings is not { IsLocked: false } meter) return;
        _ = Service.Framework.RunOnTick(() =>
        {
            if (!IsCurrentMeter(node, meter) || !node.IsEditModeReady
                || meter.IsLocked || !System.Config.Meters.Contains(meter)) return;

            meter.Position = node.Position;
            if (!meter.IsCollapsed) meter.Size = node.Size;
            meter.IsLocked = true;
            RefreshMeter(node, meter);
            ConfigRepository.Save(System.Config);
        }, delayTicks: 1);
    }

    internal void OfferPlacement(SystemConfiguration config, MeterSettings meter)
    {
        if (isDisposed || placement != null || Service.Framework.IsFrameworkUnloading) return;
        var session = new MeterPlacementSession(config, meter);
        if (session.AppliesTo(System.Config)) placement = session;
    }

    internal MeterPlacementSession? GetPlacement(MeterSettings meter)
        => !isDisposed && placement is { } session && session.Outcome != MeterPlacementOutcome.Skip && ReferenceEquals(session.Meter, meter)
            && session.AppliesTo(System.Config) ? session : null;

    internal void CancelPlacement(MeterSettings? meter = null)
    {
        if (placement is { } session && (meter == null || ReferenceEquals(session.Meter, meter)))
            session.Cancel();
    }

    internal void Tick()
    {
        if (isDisposed || Service.Framework.IsFrameworkUnloading) return;
        foreach (var nodeToCheck in activeMeters.Values)
            nodeToCheck.CheckEditAttachment();

        if (placement is not { } session) return;
        var hasNode = activeMeters.TryGetValue(session.Meter.Id, out var node) && ReferenceEquals(node.Placement, session);
        if (!Service.ClientState.IsLoggedIn || !session.AppliesTo(System.Config)
            || (session.DialogOpened && (!hasNode || !node!.IsAttachedToHud)))
            session.Cancel();

        if (session.Outcome.HasValue)
        {
            CompletePlacement(session);
            return;
        }

        if (session.DialogOpened || System.SetupWizard.IsOpen) return;
        if (!hasNode)
        {
            if (setupFramework == null) CompletePlacement(session);
            return;
        }
        if (!node!.IsEditModeReady) return;

        var dialog = System.MeterPlacementAddon;
        dialog.Completed = session.RequestOutcome;
        try
        {
            session.DialogOpened = dialog.TryOpenForPlacement();
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to open meter placement.");
        }

        if (!session.DialogOpened) CompletePlacement(session);
    }

    private void CompletePlacement(MeterPlacementSession session)
    {
        placement = null;
        if (activeMeters.TryGetValue(session.Meter.Id, out var node) && ReferenceEquals(node.Placement, session))
        {
            if (node.IsEditModeReady && session.Outcome is (MeterPlacementOutcome.Lock or MeterPlacementOutcome.KeepMovable))
            {
                session.Position = node.Position;
                session.Size = node.Size;
                if (session.TryCommit(System.Config))
                    ConfigRepository.Save(System.Config);
            }

            RefreshMeter(node, session.Meter);
        }

        var dialog = System.MeterPlacementAddon;
        dialog.Completed = null;
        dialog.CloseWithoutCompletion();
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

