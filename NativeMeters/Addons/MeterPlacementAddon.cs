using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Addons;

internal enum MeterPlacementOutcome { Skip, KeepMovable, Lock }

internal sealed class MeterPlacementAddon : NativeAddon
{
    private object? pendingPlacement;

    public Action<MeterPlacementOutcome>? Completed { get; set; }

    public unsafe bool TryOpenForPlacement()
    {
        if (InternalAddon is not null) return false;

        pendingPlacement = new object();
        try
        {
            Open();
        }
        catch
        {
            pendingPlacement = null;
            throw;
        }

        if (InternalAddon is null) pendingPlacement = null;
        return pendingPlacement is not null;
    }

    public void CloseWithoutCompletion()
    {
        pendingPlacement = null;
        Close();
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> values)
    {
        if (pendingPlacement is not { } placement) return;

        const float buttonGap = 8;
        const float keepWidth = 128;
        const float lockWidth = 132;
        var start = ContentStartPosition + new Vector2(DialogInset, 8);
        var width = ContentSize.X - 2 * DialogInset;
        var right = start.X + width;
        var buttonY = ContentStartPosition.Y + ContentSize.Y - FooterInset;

        AddLine("Drag the meter to move it. Drag a corner to resize.", start.Y);
        AddLine("The meter shows sample data when no fight is selected.", start.Y + TextHeight + 2);

        var skip = AddButton("Skip", 1, start.X, 72, MeterPlacementOutcome.Skip);
        var keep = AddButton("Keep movable", 2, right - lockWidth - buttonGap - keepWidth, keepWidth, MeterPlacementOutcome.KeepMovable);
        var finish = AddButton("Lock and finish", 3, right - lockWidth, lockWidth, MeterPlacementOutcome.Lock);
        skip.NavRight = 2;
        keep.NavLeft = 1;
        keep.NavRight = 3;
        finish.NavLeft = 2;

        var screen = (Vector2)AtkStage.Instance()->ScreenSize;
        var scaledSize = Size * addon->Scale;
        SetWindowPosition(Vector2.Max(Vector2.Zero, new Vector2(
            (screen.X - scaledSize.X) / 2, screen.Y * 0.94f - scaledSize.Y)));

        void AddLine(string text, float y)
        {
            var line = Line(text);
            line.Position = new Vector2(start.X, y);
            line.Width = width;
            line.AttachNode(this);
        }

        TextButtonNode AddButton(string label, int index, float x, float buttonWidth, MeterPlacementOutcome outcome)
        {
            var button = FooterButton(label, index, buttonWidth, () =>
            {
                if (ReferenceEquals(pendingPlacement, placement)) Complete(outcome);
            });
            button.Position = new Vector2(x, buttonY);
            button.AttachNode(this);
            return button;
        }
    }

    protected override unsafe void OnHide(AtkUnitBase* addon) => Complete(MeterPlacementOutcome.Skip);

    protected override unsafe void OnFinalize(AtkUnitBase* addon) => Complete(MeterPlacementOutcome.Skip);

    private void Complete(MeterPlacementOutcome outcome)
    {
        if (pendingPlacement is null) return;

        pendingPlacement = null;
        Completed?.Invoke(outcome);
    }
}
