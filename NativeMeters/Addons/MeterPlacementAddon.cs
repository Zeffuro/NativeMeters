using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Addons;

internal enum MeterPlacementOutcome { Skip, KeepMovable, Lock }

internal sealed class MeterPlacementAddon : NativeAddon
{
    private const float MinimumWidth = 448;
    private const float MinimumHeight = 152;
    private const float PreferredMaximumWidth = 520;
    private const float InstructionGap = 2;
    private const float InstructionClearance = 8;
    private object? pendingPlacement;

    public Action<MeterPlacementOutcome>? Completed { get; set; }

    public MeterPlacementAddon() => Size = new Vector2(MinimumWidth, MinimumHeight);

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

        this.SetLocalizedTitle("WindowPlacement");
        const float buttonGap = 8;
        var start = ContentStartPosition + new Vector2(DialogInset, 8);
        var screen = (Vector2)AtkStage.Instance()->ScreenSize;
        var moveText = Loc.Get("PlacementMove");
        var sampleText = Loc.Get("PlacementSample");
        var move = CreateInstruction(moveText);
        var sample = CreateInstruction(sampleText);
        var horizontalPadding = Size.X - ContentSize.X + 2 * DialogInset;
        var verticalPadding = Size.Y - ContentSize.Y;
        var skip = CreateButton(Loc.Get("PlacementSkip"), 1, 72, MeterPlacementOutcome.Skip);
        var keep = CreateButton(Loc.Get("PlacementMovable"), 2, 128, MeterPlacementOutcome.KeepMovable);
        var finish = CreateButton(Loc.Get("PlacementLock"), 3, 132, MeterPlacementOutcome.Lock);
        var footerWidth = skip.Width + keep.Width + finish.Width + 2 * buttonGap;
        var measuredWidth = Math.Max(move.GetTextDrawSize(considerScale: false).X, sample.GetTextDrawSize(considerScale: false).X);
        var minimumWidth = Math.Max(MinimumWidth, footerWidth + horizontalPadding);
        var widthLimit = Math.Max(minimumWidth, Math.Min(PreferredMaximumWidth, screen.X / addon->Scale));
        var windowWidth = MathF.Floor(Math.Clamp(measuredWidth + horizontalPadding + 4, minimumWidth, widthLimit));
        var width = windowWidth - horizontalPadding;
        var right = start.X + width;

        var moveBottom = PlaceInstruction(move, moveText, start.Y);
        var sampleBottom = PlaceInstruction(sample, sampleText, moveBottom + InstructionGap);
        var minimumButtonY = ContentStartPosition.Y + MinimumHeight - verticalPadding - FooterInset;
        var buttonY = Math.Max(minimumButtonY, sampleBottom + InstructionClearance);
        var windowHeight = buttonY - ContentStartPosition.Y + verticalPadding + FooterInset;
        SetWindowSize(windowWidth, windowHeight);

        PlaceButton(skip, start.X);
        PlaceButton(keep, right - finish.Width - buttonGap - keep.Width);
        PlaceButton(finish, right - finish.Width);
        skip.NavRight = 2;
        keep.NavLeft = 1;
        keep.NavRight = 3;
        finish.NavLeft = 2;

        var scaledSize = Size * addon->Scale;
        SetWindowPosition(Vector2.Max(Vector2.Zero, new Vector2(
            (screen.X - scaledSize.X) / 2, screen.Y * 0.94f - scaledSize.Y)));

        float PlaceInstruction(TextNode line, string text, float y)
        {
            line.Position = new Vector2(start.X, y);
            line.Size = new Vector2(width, TextHeight);
            line.TextFlags |= TextFlags.WordWrap;
            line.String = text;
            line.Height = Math.Max(TextHeight, line.GetTextDrawSize(considerScale: false).Y + 2);
            line.AttachNode(this);
            return y + line.Height;
        }

        TextButtonNode CreateButton(string label, int index, float buttonWidth, MeterPlacementOutcome outcome)
        {
            var button = FooterButton(label, index, buttonWidth, () =>
            {
                if (ReferenceEquals(pendingPlacement, placement)) Complete(outcome);
            });
            var labelNode = button.LabelNode;
            var labelPadding = button.Width - labelNode.Width;
            button.Width = Math.Max(buttonWidth, labelNode.GetTextDrawSize(considerScale: false).X + labelPadding + 4);
            return button;
        }

        void PlaceButton(TextButtonNode button, float x)
        {
            button.Position = new Vector2(x, buttonY);
            button.AttachNode(this);
        }
    }

    private static TextNode CreateInstruction(string text)
    {
        var line = Line(string.Empty);
        line.TextFlags = TextFlags.MultiLine;
        line.String = text;
        return line;
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
