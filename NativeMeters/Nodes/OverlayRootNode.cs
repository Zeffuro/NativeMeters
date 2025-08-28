using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using System;
using System.Collections.Generic;
using System.Numerics;
using NativeMeters.Services;

namespace NativeMeters.Nodes;

public class OverlayRootNode : SimpleOverlayNode
{
    private readonly List<NodeBase> _overlays = [];
    private NativeController _nativeController;

    public OverlayRootNode(Vector2 screenSize, NativeController controller)
    {
        NodeId = 900002;
        IsVisible = true;
        Position = Vector2.Zero;
        Size = screenSize;

        _nativeController = controller;
    }

    public void AddOverlay(NodeBase overlay)
    {
        _overlays.Add(overlay);
    }

    public unsafe void AttachAllToNativeController(AtkResNode* addonRoot)
    {
        _nativeController.AttachNode(this, addonRoot);

        foreach (var overlay in _overlays)
        {
            _nativeController.AttachNode(overlay, this);
        }
    }

    public void DetachAllFromNativeController()
    {
        foreach (var overlay in _overlays)
        {
            Service.NativeController.DetachNode(overlay);
        }
        Service.NativeController.DetachNode(this);
    }

    public void DisposeAllOverlays()
    {
        foreach (var overlay in _overlays)
        {
            (overlay as IDisposable)?.Dispose();
        }
        _overlays.Clear();
    }

    public void Cleanup()
    {
        DetachAllFromNativeController();
        DisposeAllOverlays();
    }
}