using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Extensions;
using NativeMeters.Localization;
using NativeMeters.Models;
using NativeMeters.Nodes.Configuration;
using NativeMeters.Nodes.Configuration.Setup;
using NativeMeters.Services;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Addons;

internal sealed class SetupWizardAddon : NativeAddon
{
    private enum Step { Connection, Display, Meter, Options }

    private const float HeadingHeight = 24;
    private const float HeadingGap = 10;
    private const float FooterButtonGap = 8;
    private const int PageNavStart = 16;
    private const int PageNavSpacing = 48;

    private readonly TabbedVerticalListNode?[] pages = new TabbedVerticalListNode[Enum.GetValues<Step>().Length];
    private SystemConfiguration? config;
    private SetupSampleFight? sample;
    private bool firstSetup;
    private bool isClosing;
    private bool addMeter, partyList, dtr, personalDps, replaceYou, privacy, clickthrough;
    private Step current;
    private SetupConnectionNode connection = null!;
    private SetupMeterNode? meterEditor;
    private CategoryTextNode heading = null!;
    private TextButtonNode back = null!, next = null!, cancel = null!;
    private TabbedVerticalListNode meterExtras = null!;
    private TextNode noMeterHelp = null!;

    public void OpenForSetup()
    {
        if (IsOpen) return;
        System.OverlayManager.CancelPlacement();
        System.AddonConfigurationWindow.Close();
        Open();
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> values)
    {
        this.SetLocalizedTitle("WindowSetup");
        ResetDraft(System.Config);
        BuildWindow();
        ShowPage(Step.Connection);
    }

    private void ResetDraft(SystemConfiguration settings)
    {
        config = settings;
        isClosing = false;
        firstSetup = settings.SetupStatus == SetupStatus.Pending;
        addMeter = true;
        partyList = settings.PartyListMeter.Enabled;
        dtr = settings.DtrSettings.Enabled;
        personalDps = settings.DtrSettings.UsePersonalDps;
        replaceYou = settings.General.ReplaceYou;
        privacy = settings.General.PrivacyMode;
        clickthrough = false;
    }

    private void BuildWindow()
    {
        var start = ContentStartPosition + new Vector2(DialogInset);
        var width = ContentSize.X - DialogInset * 2;
        var buttonY = ContentStartPosition.Y + ContentSize.Y - FooterInset;
        heading = Section("");
        heading.Position = start;
        heading.Size = new Vector2(width, HeadingHeight);
        heading.AttachNode(this);

        cancel = AddButton(firstSetup ? Loc.Get("SetupSkip") : Loc.Get("SetupCancel"), 1, start.X, Cancel);
        back = AddButton(Loc.Get("SetupBack"), 2, start.X + width - ButtonWidth * 2 - FooterButtonGap, Back);
        next = AddButton(Loc.Get("SetupNext"), 3, start.X + width - ButtonWidth, Next);

        TextButtonNode AddButton(string text, int navIndex, float x, Action click)
        {
            var button = FooterButton(text, navIndex, ButtonWidth, click);
            button.Position = new Vector2(x, buttonY);
            button.AttachNode(this);
            return button;
        }
    }

    private TabbedVerticalListNode BuildPage(Step step, SystemConfiguration settings)
    {
        var start = heading.Position + new Vector2(0, heading.Height + HeadingGap);
        var width = heading.Width;
        var page = step switch
        {
            Step.Connection => connection = new SetupConnectionNode(width, settings.ConnectionSettings, RefreshNavigation),
            Step.Display => BuildDisplay(width, sample ??= new SetupSampleFight(), settings),
            Step.Meter => meterEditor = new SetupMeterNode(width, cancel.Y - start.Y - FooterClearance, sample ??= new SetupSampleFight(), RefreshNavigation),
            _ => BuildOptions(width),
        };
        page.Position = start;
        page.AttachNode(this);
        return page;
    }

    protected override unsafe void OnUpdate(AtkUnitBase* addon)
    {
        if (isClosing || config == null || !addon->IsVisible || Service.Framework.IsFrameworkUnloading) return;
        if (current == Step.Connection) connection.Tick();
        if (current == Step.Meter) meterEditor?.UpdatePreview();
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
    {
        isClosing = true;
        meterEditor?.CollapseChoices();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        config = null;
        Array.Clear(pages);
        sample = null;
        meterEditor = null;
        connection = null!;
        heading = null!;
        meterExtras = null!;
        noMeterHelp = null!;
        back = next = cancel = null!;
    }

    private TabbedVerticalListNode BuildDisplay(float width, SetupSampleFight sample, SystemConfiguration settings)
    {
        var page = Page(width);
        var previewWidth = width - page.TabSize * 2 - page.ItemSpacing;
        var format = settings.DtrSettings.FormatString;
        var dtrPreview = SetupHudPreview.ServerInfo(sample, format, personalDps, previewWidth);
        var personalCheck = Check(Loc.Get("SetupPersonalDps"), personalDps, value =>
        {
            personalDps = value;
            dtrPreview.String = SetupHudPreview.FormatServerInfo(sample, format, value);
        });
        personalCheck.TextTooltip = Loc.Get("SetupPersonalDpsHelp");
        personalCheck.IsVisible = dtr;
        dtrPreview.IsVisible = dtr;

        page.AddNode(1, Check(Loc.Get("SetupAddMeter"), addMeter, value => addMeter = value));
        page.AddNode(2, Line(Loc.Get("SetupAddMeterHelp")));
        page.AddDummy(5);
        page.AddNode(1, Check(Loc.Get("SetupPartyListDps"), partyList, value => partyList = value));
        page.AddDummy(5);
        page.AddNode(1, Check(Loc.Get("SetupServerDps"), dtr, value =>
        {
            dtr = value;
            personalCheck.IsVisible = value;
            dtrPreview.IsVisible = value;
            page.RecalculateLayout();
            RefreshNavigation();
        }));
        page.AddNode(2, [personalCheck, dtrPreview]);
        return page;
    }

    private TabbedVerticalListNode BuildOptions(float width)
    {
        var page = Page(width);
        page.AddNode(1, [
            Check(Loc.Get("SetupReplaceYou"), replaceYou, value => replaceYou = value),
            Check(Loc.Get("SetupPrivacy"), privacy, value => privacy = value),
        ]);
        page.AddDummy(10);

        meterExtras = Page(width);
        meterExtras.AddNode(Section(Loc.Get("SetupStandalone")));
        var clickthroughCheck = Check(Loc.Get("SetupClickthrough"), clickthrough, value => clickthrough = value);
        clickthroughCheck.TextTooltip = Loc.Get("SetupClickthroughHelp");
        meterExtras.AddNode(1, clickthroughCheck);
        meterExtras.AddDummy(6);
        meterExtras.AddNode(1, [
            Line(Loc.Get("SetupTryDummy")),
            Line(Loc.Get("SetupUnlockHelp")),
        ]);
        page.AddNode(meterExtras);
        noMeterHelp = Line(Loc.Get("SetupSettingsHelp"));
        page.AddNode(1, noMeterHelp);
        return page;
    }

    private void ShowPage(Step step)
    {
        if (config == null) return;
        if (current == Step.Meter) meterEditor?.CollapseChoices();
        var active = pages[(int)step] ??= BuildPage(step, config);
        current = step;
        foreach (var page in pages)
            if (page != null) page.IsVisible = page == active;

        heading.String = step switch
        {
            Step.Connection => Loc.Get("SetupChooseParser"),
            Step.Display => Loc.Get("SetupChooseDisplay"),
            Step.Meter => Loc.Get("SetupEditMeter"),
            _ => Loc.Get("SetupExtras"),
        };
        back.IsVisible = step != Step.Connection;
        cancel.NavRight = back.IsVisible ? 2 : 3;
        back.NavLeft = 1;
        back.NavRight = 3;
        next.NavLeft = back.IsVisible ? 2 : 1;
        next.String = step == Step.Options ? Loc.Get("SetupFinish") : Loc.Get("SetupNext");

        if (step == Step.Options)
        {
            meterExtras.IsVisible = addMeter;
            noMeterHelp.IsVisible = !addMeter;
        }

        active.RecalculateLayout();
        RefreshNavigation();
    }

    private void RefreshNavigation()
    {
        if (config == null || pages[(int)current] is not { } active) return;
        var enabled = current switch
        {
            Step.Connection => connection.CanContinue,
            Step.Meter => meterEditor?.CanContinue == true,
            _ => true,
        };
        next.IsEnabled = enabled;
        var start = PageNavStart + (int)current * PageNavSpacing;
        var footer = enabled ? 3 : back.IsVisible ? 2 : 1;
        var end = active is SetupMeterNode editor
            ? editor.ApplyNavigation(start, footer)
            : ConfigurationNavigation.Apply(active, start, footer, footer);
        cancel.NavUp = back.NavUp = next.NavUp = end > start ? end - 1 : 0;
        cancel.NavDown = back.NavDown = next.NavDown = end > start ? start : 0;
    }

    private void Back()
        => ShowPage(current == Step.Options && !addMeter ? Step.Display : current - 1);

    private void Next()
    {
        if (!next.IsEnabled) return;
        if (current == Step.Options) Finish();
        else ShowPage(current == Step.Display && !addMeter ? Step.Options : current + 1);
    }

    private unsafe void Finish()
    {
        if (config is null || !ReferenceEquals(config, System.Config))
        {
            Close();
            return;
        }

        MeterSettings? meter = null;
        if (addMeter && meterEditor != null)
        {
            meter = meterEditor.CreateMeter();
            meter.IsEnabled = true;
            meter.IsLocked = true;
            meter.IsClickthrough = clickthrough;
            meter.IsCollapsed = false;
            var stage = AtkStage.Instance();
            var screen = new Vector2(stage->ScreenSize.Width, stage->ScreenSize.Height);
            FitNewMeter(meter, screen);
        }

        System.AddonConfigurationWindow.Close();
        ConnectionSelection.Apply(connection.Source,
            connection.Source == ConnectionType.WebSocket ? connection.WebSocketUrl.Trim() : null);
        if (meter is not null) config.Meters.Add(meter);
        if (addMeter || (partyList && !config.PartyListMeter.Enabled)) config.General.IsEnabled = true;
        config.PartyListMeter.Enabled = partyList;
        config.DtrSettings.Enabled = dtr;
        config.DtrSettings.UsePersonalDps = personalDps;
        config.General.ReplaceYou = replaceYou;
        config.General.PrivacyMode = privacy;
        if (firstSetup) config.SetupStatus = SetupStatus.Finished;
        ConfigRepository.Save(config);
        if (meter != null) System.OverlayManager.OfferPlacement(config, meter);
        config = null;
        Close();

        System.OverlayManager.Setup();
        System.PartyListMeterManager.UpdateSettings();
        System.DtrService.UpdateBar();
        Service.NotificationManager.Success(Loc.Get("SetupSaved"));
    }

    private void Cancel()
    {
        if (firstSetup && ReferenceEquals(config, System.Config))
        {
            config!.SetupStatus = SetupStatus.Default;
            config.EnsureInitialized();
            ConfigRepository.Save(config);
            System.OverlayManager.Setup();
        }
        config = null;
        Close();
    }

    private static void FitNewMeter(MeterSettings meter, Vector2 screen)
    {
        const float screenMargin = 16;
        const float horizontalYRatio = 0.12f;
        var maxScale = Math.Min((screen.X - 2 * screenMargin) / meter.Size.X, (screen.Y - 2 * screenMargin) / meter.Size.Y);
        meter.Scale = Math.Clamp(Math.Min(meter.Scale, (int)(maxScale * 100)), 10, 400);
        var actualSize = meter.Size * (meter.Scale / 100f);
        var maxPosition = Vector2.Max(Vector2.Zero, screen - actualSize - new Vector2(screenMargin));
        if (meter.Layout == MeterLayout.Horizontal)
            meter.Position = new Vector2((screen.X - actualSize.X) / 2, screen.Y * horizontalYRatio);
        meter.Position = Vector2.Clamp(meter.Position, Vector2.Zero, maxPosition);
    }
}
