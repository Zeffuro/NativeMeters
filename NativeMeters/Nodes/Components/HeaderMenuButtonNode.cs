using System.Numerics;
using KamiToolKit.Enums;
using KamiToolKit.ContextMenu;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Addons;
using NativeMeters.Configuration;
using NativeMeters.Localization;
using NativeMeters.Services;
using NativeMeters.Utilities;

namespace NativeMeters.Nodes.Components;

public class HeaderMenuButtonNode : SimpleComponentNode
{
    private readonly record struct ButtonAppearance(CircleButtonIcon Icon, string Tooltip);

    private readonly CircleButtonNode _menuButton;
    private readonly ContextMenu _contextMenu;
    private ButtonAppearance currentAppearance = new(CircleButtonIcon.GearCog, "Context Menu");

    public MeterSettings? MeterSettings { get; set; }
    public MeterButtonAction ButtonAction { get; private set; }

    public HeaderMenuButtonNode()
    {
        _contextMenu = new ContextMenu();

        _menuButton = new CircleButtonNode
        {
            Icon = CircleButtonIcon.GearCog,
            Size = new Vector2(24f),
            TextTooltip = Loc.EnumLabel(MeterButtonAction.ContextMenu),
            OnClick = ExecuteAction
        };
        _menuButton.AttachNode(this);
    }

    public void Configure(MeterSettings? meterSettings, MeterButtonAction action)
    {
        MeterSettings = meterSettings;
        ButtonAction = action;

        ButtonAppearance appearance = action switch
        {
            MeterButtonAction.ToggleCollapse when meterSettings?.IsCollapsed == true => new(CircleButtonIcon.ArrowDown, "Expand Meter"),
            MeterButtonAction.ToggleCollapse => new(CircleButtonIcon.UpArrow, "Collapse Meter"),
            MeterButtonAction.ResetDummies => new(CircleButtonIcon.Refresh, "Reset All Dummies"),
            MeterButtonAction.DetailedBreakdown => new(CircleButtonIcon.Document, "Detailed Breakdown"),
            MeterButtonAction.Settings => new(CircleButtonIcon.ActiveGearCog, "Settings"),
            MeterButtonAction.CopyEncounterSummary => new(CircleButtonIcon.PinPaper, "Copy Encounter Summary"),
            _ => new(CircleButtonIcon.GearCog, "Context Menu"),
        };

        if (currentAppearance == appearance) return;
        _menuButton.Icon = appearance.Icon;
        _menuButton.TextTooltip = appearance.Tooltip;
        currentAppearance = appearance;
    }

    private void ExecuteAction()
    {
        if (MeterSettings is { } meter && System.OverlayManager.GetPlacement(meter) != null) return;

        switch (ButtonAction)
        {
            case MeterButtonAction.ToggleCollapse:
                if (MeterSettings != null) MeterSettings.IsCollapsed = !MeterSettings.IsCollapsed;
                break;
            case MeterButtonAction.ResetDummies:
                EnmityUtilities.ResetAllStrikingDummies();
                break;
            case MeterButtonAction.DetailedBreakdown:
                System.AddonDetailedBreakdownWindow.Toggle();
                break;
            case MeterButtonAction.Settings:
                System.AddonConfigurationWindow.Toggle();
                break;
            case MeterButtonAction.CopyEncounterSummary:
                ClipboardService.CopyEncounterSummary();
                break;
            default:
                OpenContextMenu();
                break;
        }
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

    protected override void Dispose(bool isNativeDestructor)
    {
        if (!isNativeDestructor)
        {
            _contextMenu.Dispose();
        }

        base.Dispose(isNativeDestructor);
    }
}
