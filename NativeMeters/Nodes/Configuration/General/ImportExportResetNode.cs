using System.IO;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration.ImportExport;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class ImportExportResetNode : HorizontalListNode
{
    public ImportExportResetNode()
    {
        Height = 0;
        Width = 760;
        Alignment = HorizontalListAnchor.Right;
        FirstItemSpacing = 3;
        ItemSpacing = 2;
        IsVisible = true;

        AddNode(new ImGuiIconButtonNode {
            Y = 3,
            Height = 30,
            Width = 30,
            IsVisible = true,
            TextTooltip = Loc.Get("GeneralImport"),
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\download.png"),
            OnClick = ImportConfig
        });

        AddNode(new ImGuiIconButtonNode {
            Y = 3,
            Height = 30,
            Width = 30,
            IsVisible = true,
            TextTooltip = Loc.Get("GeneralExport"),
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\upload.png"),
            OnClick = ExportConfig
        });

        AddNode(new HoldButtonNode {
            IsVisible = true,
            Y = 0,
            Height = 32,
            Width = 100,
            String = Loc.Get("GeneralReset"),
            TextNode = { TextColor = ColorHelper.GetColor(50) },
            TextTooltip = Loc.Get("GeneralResetHelp"),
            OnClick = ResetConfig
        });
    }

    private static void ResetConfig()
    {
        ConfigPorter.TryResetConfig();
        System.OverlayManager.Setup();
        System.PartyListMeterManager?.UpdateSettings();
        System.AddonConfigurationWindow.Close();
    }

    private static void ImportConfig()
    {
        if (!Service.KeyState[VirtualKey.SHIFT]) return;

        ConfigPorter.TryImportConfigFromClipboard();
        System.AddonConfigurationWindow.Close();
        System.OverlayManager.Setup();
        System.PartyListMeterManager?.UpdateSettings();
    }

    private static void ExportConfig() => ConfigPorter.TryExportConfigToClipboard(System.Config);
}
