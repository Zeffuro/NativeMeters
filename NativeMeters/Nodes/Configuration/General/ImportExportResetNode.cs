using System.IO;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Helpers;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class ImportExportResetNode : HorizontalListNode
{
    public ImportExportResetNode()
    {
        Height = 0;
        Width = 600;
        Alignment = HorizontalListAnchor.Right;
        FirstItemSpacing = 3;
        ItemSpacing = 2;
        IsVisible = true;

        AddNode(new ImGuiIconButtonNode {
            Y = 3,
            Height = 30,
            Width = 30,
            IsVisible = true,
            TextTooltip = " Import Configuration\n(hold shift to confirm)",
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\download.png"),
            OnClick = ImportConfig
        });

        AddNode(new ImGuiIconButtonNode {
            Y = 3,
            Height = 30,
            Width = 30,
            IsVisible = true,
            TextTooltip = "Export Configuration",
            TexturePath = Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, @"Assets\Icons\upload.png"),
            OnClick = ExportConfig
        });

        AddNode(new HoldButtonNode {
            IsVisible = true,
            Y = 0,
            Height = 32,
            Width = 100,
            String = "Reset",
            TextNode = { TextColor = ColorHelper.GetColor(50) },
            TextTooltip = "   Reset configuration\n(hold button to confirm)",
            OnClick = ResetConfig
        });
    }

    private static void ResetConfig()
    {
        ImportExportResetHelper.TryResetConfig();
        System.AddonConfigurationWindow.Close();
    }

    private static void ImportConfig()
    {
        if (!Service.KeyState[VirtualKey.SHIFT]) return;

        ImportExportResetHelper.TryImportConfigFromClipboard();
        System.AddonConfigurationWindow.Close();
    }

    private static void ExportConfig() => ImportExportResetHelper.TryExportConfigToClipboard(System.Config);
}