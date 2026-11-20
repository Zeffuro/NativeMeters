using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;
using NativeMeters.Localization;

namespace NativeMeters.Nodes.Configuration.Meter;

internal sealed class MeterImportConfirmationAddon(NativeAddon owner) : NativeAddon
{
    private sealed record RequestData(string MeterName, Action Apply);

    private RequestData? request;
    private int blockedOwnerId;

    public unsafe bool IsBusy => InternalAddon is not null || request is not null;

    public unsafe void Request(string name, Action onConfirm)
    {
        if (IsBusy || !owner.IsOpen) return;

        var meterName = string.IsNullOrWhiteSpace(name) ? Loc.Get("MeterUnnamed") : name.Replace('\r', ' ').Replace('\n', ' ');
        request = new RequestData(meterName, onConfirm);
        try
        {
            Open();
        }
        finally
        {
            if (InternalAddon is null) ClearRequest();
        }
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> values)
    {
        if (request is not { } pending) return;

        this.SetLocalizedTitle("WindowImportMeter");
        var start = ContentStartPosition + new Vector2(8, 8);
        var width = ContentSize.X - 16;
        var description = Line(Loc.Get("MeterReplacePrompt"));
        description.Position = start;
        description.Width = width;
        description.AttachNode(this);

        var name = Line(pending.MeterName);
        name.Position = start + new Vector2(0, TextHeight + 2);
        name.Width = width;
        name.AttachNode(this);

        var buttonY = ContentStartPosition.Y + ContentSize.Y - 32;
        var cancel = FooterButton(Loc.Get("MeterCancel"), 1, ButtonWidth, () =>
        {
            if (ReferenceEquals(request, pending)) Cancel();
        });
        cancel.Position = new Vector2(start.X + width - ButtonWidth * 2 - 8, buttonY);
        cancel.NavRight = 2;
        cancel.AttachNode(this);

        var replace = FooterButton(Loc.Get("MeterReplace"), 2, ButtonWidth, () => Confirm(pending));
        replace.Position = new Vector2(start.X + width - ButtonWidth, buttonY);
        replace.NavLeft = 1;
        replace.AttachNode(this);
    }

    protected override unsafe void OnShow(AtkUnitBase* addon)
    {
        AtkUnitBase* ownerAddon = owner;
        if (ownerAddon is null || request is null || blockedOwnerId != 0 || ownerAddon->NumBlockingAddons == byte.MaxValue) return;

        ownerAddon->NumBlockingAddons++;
        blockedOwnerId = ownerAddon->Id;
    }

    protected override unsafe void OnHide(AtkUnitBase* addon) => ClearRequest();

    protected override unsafe void OnFinalize(AtkUnitBase* addon) => ClearRequest();

    public void Cancel()
    {
        if (request is null) return;

        ClearRequest();
        Close();
    }

    private void Confirm(RequestData pending)
    {
        if (!ReferenceEquals(request, pending)) return;

        Cancel();
        pending.Apply();
    }

    private unsafe void ClearRequest()
    {
        request = null;

        if (blockedOwnerId == 0) return;
        AtkUnitBase* ownerAddon = owner;
        if (ownerAddon is not null && ownerAddon->Id == blockedOwnerId && ownerAddon->NumBlockingAddons > 0)
            ownerAddon->NumBlockingAddons--;

        blockedOwnerId = 0;
    }
}
