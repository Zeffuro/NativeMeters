using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using Lumina.Data.Parsing.Uld;
using NativeMeters.Configuration;
using NativeMeters.Configuration.ImportExport;
using NativeMeters.Configuration.Presets;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class AddMeterDialogAddon : NativeAddon
{
    public Action<MeterSettings>? OnMeterCreated { get; set; }

    private VerticalListNode? layoutNode;
    private TextDropDownNode? presetDropdown;

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        var presetNames = MeterPresets.GetPresetNames();
        var presetRowButtonWidth = 132.0f;
        var presetDropdownWidth = Math.Max(160.0f, ContentSize.X - presetRowButtonWidth - 8.0f);

        layoutNode = new VerticalListNode
        {
            Position = ContentStartPosition,
            Size = ContentSize,
            FitWidth = true,
            ItemSpacing = 10.0f,
            InitialNodes =
            [
                new TextNode
                {
                    Height = 48.0f,
                    String = "Create a meter from a preset or import meter data from the clipboard.",
                    TextColor = ColorHelper.GetColor(8),
                    TextOutlineColor = ColorHelper.GetColor(7),
                    TextFlags = TextFlags.Edge | TextFlags.MultiLine,
                },
                new TextButtonNode
                {
                    Height = 28.0f,
                    String = "Import from Clipboard",
                    OnClick = ImportFromClipboard,
                    NavIndex = 1,
                    NavUp = 4,
                    NavDown = 2,
                },
                new HorizontalListNode
                {
                    Height = 28.0f,
                    FitHeight = true,
                    ItemSpacing = 8.0f,
                    InitialNodes =
                    [
                        presetDropdown = new TextDropDownNode
                        {
                            Width = presetDropdownWidth,
                            Options = presetNames,
                            SelectedOption = presetNames.FirstOrDefault(),
                            NavIndex = 2,
                            NavUp = 1,
                            NavDown = 4,
                            NavRight = 3,
                        },
                        new TextButtonNode
                        {
                            Width = presetRowButtonWidth,
                            String = "Use Preset",
                            OnClick = CreateFromPreset,
                            NavIndex = 3,
                            NavUp = 1,
                            NavDown = 4,
                            NavLeft = 2,
                        },
                    ],
                },
                new TextButtonNode
                {
                    Height = 28.0f,
                    TextId = 2, // Cancel
                    SheetType = NodeData.SheetType.Addon,
                    OnClick = Close,
                    NavIndex = 4,
                    NavUp = 2,
                    NavDown = 1,
                },
            ],
        };
        layoutNode.AttachNode(this);
        addon->UldManager.SetupTextRecursive();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        layoutNode = null;
        presetDropdown = null;

        base.OnFinalize(addon);
    }

    private void CreateFromPreset()
    {
        var presetName = presetDropdown?.SelectedOption;
        if (string.IsNullOrWhiteSpace(presetName)) return;

        var newMeter = CreateNewMeter();
        MeterPresets.ApplyPreset(presetName, newMeter);
        EnsureName(newMeter);

        OnMeterCreated?.Invoke(newMeter);
        Close();
    }

    private void ImportFromClipboard()
    {
        var importedMeter = ConfigPorter.TryImportMeterFromClipboard();
        if (importedMeter is null) return;

        var newMeter = CreateNewMeter();
        MeterPresets.ApplySettings(importedMeter, newMeter);
        EnsureName(newMeter);

        OnMeterCreated?.Invoke(newMeter);
        Close();
    }

    private static MeterSettings CreateNewMeter()
        => new() { Name = CreateNewMeterName() };

    private static void EnsureName(MeterSettings meter)
    {
        if (string.IsNullOrWhiteSpace(meter.Name))
        {
            meter.Name = CreateNewMeterName();
        }
    }

    private static string CreateNewMeterName()
        => $"New Meter {System.Config.Meters.Count + 1}";
}
