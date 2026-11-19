using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.ImportExport;
using NativeMeters.Configuration.Presets;
using NativeMeters.Nodes.Configuration.Setup;
using NativeMeters.Services;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class AddMeterDialogAddon : NativeAddon
{
    public Action<MeterSettings>? OnMeterCreated { get; set; }

    private SetupMeterNode? editor;
    private TextButtonNode cancel = null!, import = null!, add = null!;
    private bool closing;

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> values)
    {
        const float importWidth = 176;
        const float buttonGap = 12;
        closing = false;
        var start = ContentStartPosition + new Vector2(DialogInset);
        var width = ContentSize.X - DialogInset * 2;
        var buttonY = ContentStartPosition.Y + ContentSize.Y - FooterInset;

        editor = new SetupMeterNode(width, buttonY - start.Y - FooterClearance, new SetupSampleFight(), RefreshNavigation)
        {
            Position = start,
        };
        editor.AttachNode(this);
        cancel = AddButton("Cancel", 1, start.X, ButtonWidth, Close);
        import = AddButton("Import from Clipboard", 2, start.X + width - ButtonWidth - buttonGap - importWidth, importWidth, ImportFromClipboard);
        add = AddButton("Add Meter", 3, start.X + width - ButtonWidth, ButtonWidth, CreateFromEditor);
        RefreshNavigation();

        TextButtonNode AddButton(string label, int index, float x, float buttonWidth, Action clicked)
        {
            var button = FooterButton(label, index, buttonWidth, clicked);
            button.Position = new Vector2(x, buttonY);
            button.AttachNode(this);
            return button;
        }
    }

    protected override unsafe void OnUpdate(AtkUnitBase* addon)
    {
        if (!closing && addon->IsVisible && !Service.Framework.IsFrameworkUnloading) editor?.UpdatePreview();
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
    {
        closing = true;
        editor?.CollapseChoices();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        editor = null;
        cancel = import = add = null!;
    }

    private void RefreshNavigation()
    {
        if (editor == null) return;
        add.IsEnabled = editor.CanContinue;
        const int editorNavStart = 16;
        var end = editor.ApplyNavigation(editorNavStart, add.IsEnabled ? 3 : 1);
        cancel.NavUp = import.NavUp = add.NavUp = end - 1;
        cancel.NavDown = import.NavDown = add.NavDown = editorNavStart;
        cancel.NavRight = 2;
        import.NavLeft = 1;
        import.NavRight = add.IsEnabled ? 3 : 0;
        add.NavLeft = 2;
    }

    private unsafe void CreateFromEditor()
    {
        if (closing || editor == null || !editor.CanContinue) return;
        var meter = editor.CreateMeter(CreateNewMeterName());
        if (meter.Layout == MeterLayout.Horizontal && meter.Size.X > 0)
        {
            const float screenMargin = 16;
            var screenWidth = AtkStage.Instance()->ScreenSize.Width;
            var fitScale = (int)((screenWidth - screenMargin) * 100 / meter.Size.X);
            meter.Scale = Math.Clamp(Math.Min(meter.Scale, fitScale), 10, 400);
        }
        Create(meter);
    }

    private void ImportFromClipboard()
    {
        if (closing) return;
        var imported = ConfigPorter.TryImportMeterFromClipboard();
        if (imported == null) return;

        var meter = new MeterSettings();
        MeterPresets.ApplySettings(imported, meter);
        Create(meter);
    }

    private void Create(MeterSettings meter)
    {
        if (string.IsNullOrWhiteSpace(meter.Name)) meter.Name = CreateNewMeterName();
        closing = true;
        OnMeterCreated?.Invoke(meter);
        Close();
    }

    private static string CreateNewMeterName()
        => $"New Meter {System.Config.Meters.Count + 1}";
}
