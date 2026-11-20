using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Utility;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Clients;
using NativeMeters.Configuration;
using NativeMeters.Localization;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;
using NativeMeters.Services;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupConnectionNode : TabbedVerticalListNode
{
    private const int StatusPollIntervalMs = 750;
    private const int ProbeTimeoutSeconds = 4;
    private readonly TabbedVerticalListNode act;
    private readonly TabbedVerticalListNode iinact;
    private readonly TabbedVerticalListNode local;
    private readonly TextNode iinactInstall;
    private readonly LabeledTextButtonNode status;
    private readonly CheckboxRowNode bypassCheck;
    private readonly Action changed;
    private CancellationTokenSource? probeCancellation;
    private Task<bool>? probe;
    private bool? probeResult;
    private bool bypass;
    private long lastStatusUpdate;

    public ConnectionType Source { get; private set; }
    public string WebSocketUrl { get; private set; }
    public bool CanContinue { get; private set; }

    public SetupConnectionNode(float width, ConnectionSettings settings, Action changed)
    {
        this.changed = changed;
        Width = width;
        FitWidth = true;
        ItemSpacing = Spacing;
        TabSize = Indent;
        Source = Enum.IsDefined(settings.SelectedConnectionType) ? settings.SelectedConnectionType : ConnectionType.WebSocket;
        WebSocketUrl = settings.WebSocketUrl ?? ConnectionSettings.DefaultWebSocketUrl;

        (ConnectionType Type, string Label)[] sources =
        [
            (ConnectionType.WebSocket, "ACT"),
            (ConnectionType.IINACTIPC, "IINACT"),
            (ConnectionType.Internal, Loc.Get("SetupInternalParser")),
        ];
        var radios = new RadioButtonGroupNode { VerticalPadding = 6 };
        foreach (var (type, label) in sources)
            radios.AddButton(label, () => Select(type));
        radios.SelectedOption = sources.First(entry => entry.Type == Source).Label;
        AddNode(1, radios);
        AddDummy(5);

        act = Page(width - Indent - Spacing);
        act.AddNode([
            Line(Loc.Get("SetupActIntro")),
            Line(Loc.Get("SetupActServer")),
            Line(Loc.Get("SetupActStart")),
            new LabeledTextInputNode
            {
                LabelText = Loc.Get("SetupWebSocket"),
                Text = WebSocketUrl,
                OnInputReceived = text => ChangeUrl(text.ToString()),
                OnInputComplete = text => ChangeUrl(text.ToString()),
            },
            Guide(Loc.Get("SetupActGuide"), "https://overlayplugin.github.io/docs/setup/"),
        ]);
        iinact = Page(width - Indent - Spacing);
        iinactInstall = Line(Loc.Get("SetupIinactChecking"));
        iinact.AddNode([
            Line(Loc.Get("SetupIinactIntro")),
            iinactInstall,
            Guide(Loc.Get("SetupIinactInstall"), "https://www.iinact.com/installation/"),
        ]);
        local = Page(width - Indent - Spacing);
        local.AddNode([
            Line(Loc.Get("SetupLocalIntro")),
            Line(Loc.Get("SetupLocalDifferences")),
            Line(Loc.Get("SetupLocalNoLogs")),
        ]);
        AddNode(1, [act, iinact, local]);
        AddDummy(5);
        status = new LabeledTextButtonNode
        {
            LabelWidth = 272,
            LabelText = Loc.Get("SetupUnchecked"),
            ButtonText = Loc.Get("SetupCheckConnection"),
            OnClick = CheckConnection,
        };
        bypassCheck = Check(Loc.Get("SetupContinueOffline"), false, value =>
        {
            bypass = value;
            RefreshStatus();
        });
        AddNode(1, [status, bypassCheck]);
        UpdateSource();
    }

    private static HorizontalListNode Guide(string label, string url)
        => new()
        {
            Height = 26,
            FitHeight = true,
            InitialNodes = [new TextButtonNode { Width = 152, String = label, OnClick = () => Util.OpenLink(url) }],
        };

    private void Select(ConnectionType type)
    {
        if (Source == type) return;
        Source = type;
        ResetCheck();
        UpdateSource();
        changed();
    }

    private void ChangeUrl(string text)
    {
        if (WebSocketUrl == text) return;
        WebSocketUrl = text;
        ResetCheck();
        RefreshStatus();
    }

    private void ResetCheck()
    {
        CancelProbe();
        probeResult = null;
        bypass = false;
        bypassCheck.IsChecked = false;
    }

    private void UpdateSource()
    {
        act.IsVisible = Source == ConnectionType.WebSocket;
        iinact.IsVisible = Source == ConnectionType.IINACTIPC;
        local.IsVisible = Source == ConnectionType.Internal;
        status.IsVisible = bypassCheck.IsVisible = !local.IsVisible;
        RecalculateLayout();
        RefreshStatus();
    }

    private void CheckConnection()
    {
        if (Source == ConnectionType.WebSocket && TryGetWebSocketUri(out var uri))
        {
            CancelProbe();
            probeResult = null;
            probeCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(ProbeTimeoutSeconds));
            probe = CheckWebSocketAsync(uri, probeCancellation.Token);
        }
        RefreshStatus();
    }

    public void Tick()
    {
        if (probe is { IsCompleted: true })
        {
            probeResult = probe.IsCompletedSuccessfully && probe.Result;
            if (probe.Exception is { } exception) Service.Logger.Error(exception, "ACT connection check failed.");
            CancelProbe();
            RefreshStatus();
        }
        else if (Stopwatch.GetElapsedTime(lastStatusUpdate).TotalMilliseconds >= StatusPollIntervalMs)
        {
            RefreshStatus();
        }
    }

    private void RefreshStatus()
    {
        lastStatusUpdate = Stopwatch.GetTimestamp();
        var valid = Source != ConnectionType.WebSocket || TryGetWebSocketUri(out _);
        var ready = Source switch
        {
            ConnectionType.Internal => true,
            ConnectionType.IINACTIPC => IINACTIpcClient.IsListening(),
            _ => valid && (probeResult is true ||
                (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.WebSocket &&
                 WebSocketUrl.Trim() == System.Config.ConnectionSettings.WebSocketUrl && System.MeterService.IsConnected)),
        };

        var label = Source switch
        {
            ConnectionType.IINACTIPC when ready => Loc.Get("SetupIinactReady"),
            ConnectionType.IINACTIPC => Loc.Get("SetupIinactNotReady"),
            _ when !valid => Loc.Get("SetupInvalidUrl"),
            _ when probe != null => Loc.Get("SetupChecking"),
            _ when ready => Loc.Get("SetupConnected"),
            _ when probeResult == false => Loc.Get("SetupActUnavailable"),
            _ => Loc.Get("SetupUnchecked"),
        };
        if (status.LabelText.ToString() != label)
        {
            status.LabelText = label;
            status.LabelTextColor = ColorHelper.GetColor(ready ? 46u : !valid || probeResult == false ? 14u : 8u);
        }

        if (Source == ConnectionType.IINACTIPC)
        {
            var plugin = Service.PluginInterface.InstalledPlugins.FirstOrDefault(entry => entry.InternalName == "IINACT");
            var installation = plugin is null ? Loc.Get("SetupIinactMissing")
                : !plugin.IsLoaded ? Loc.Get("SetupIinactDisabled")
                : ready ? Loc.Get("SetupIinactEnabled") : Loc.Get("SetupIinactLoaded");
            if (iinactInstall.String.ToString() != installation) iinactInstall.String = installation;
        }

        var canContinue = valid && (ready || bypass);
        if (CanContinue == canContinue) return;
        CanContinue = canContinue;
        changed();
    }

    private bool TryGetWebSocketUri(out Uri uri)
    {
        if (Uri.TryCreate(WebSocketUrl.Trim(), UriKind.Absolute, out var parsed)
            && parsed.Scheme is "ws" or "wss" && parsed.Host.Length != 0)
        {
            uri = parsed;
            return true;
        }
        uri = null!;
        return false;
    }

    private static async Task<bool> CheckWebSocketAsync(Uri uri, CancellationToken token)
    {
        try
        {
            using var socket = new ClientWebSocket();
            await WebSocketClient.ConnectAndSubscribeAsync(socket, uri, token).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (exception is WebSocketException or OperationCanceledException
            or ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    private void CancelProbe()
    {
        probeCancellation?.Cancel();
        probeCancellation?.Dispose();
        probeCancellation = null;
        probe = null;
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        CancelProbe();
        base.Dispose(isNativeDestructor);
    }
}
