using System;
using System.Numerics;
using KamiToolKit.Classes;
using KamiToolKit.ContextMenu;
using KamiToolKit.Nodes;
using NativeMeters.Addons;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components;

public class HeaderMenuButtonNode : SimpleComponentNode
{
    private readonly CircleButtonNode _menuButton;
    private readonly ContextMenu _contextMenu;

    public MeterSettings? MeterSettings { get; set; }

    public HeaderMenuButtonNode()
    {
        _contextMenu = new ContextMenu();

        _menuButton = new CircleButtonNode
        {
            Icon = ButtonIcon.GearCog,
            Size = new Vector2(24f),
            OnClick = OpenContextMenu
        };
        _menuButton.AttachNode(this);
    }

    private void OpenContextMenu()
    {
        if (MeterSettings != null)
        {
            MeterContextMenu.Open(MeterSettings, _contextMenu);
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _menuButton.Size = new Vector2(Width, Height);
        _menuButton.Position = Vector2.Zero;
    }

    public void Dispose()
    {
        _contextMenu?.Dispose();
    }
}
