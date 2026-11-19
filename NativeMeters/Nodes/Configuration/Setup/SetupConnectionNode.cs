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
using NativeMeters.Models;
using NativeMeters.Nodes.Input;
using NativeMeters.Services;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupConnectionNode : TabbedVerticalListNode
{
    private const int StatusPollIntervalMs = 750;
    private const int ProbeTimeoutSeconds = 4;
    private static readonly (ConnectionType Type, string Label)[] Sources =
    [
        (ConnectionType.WebSocket, "ACT"),
        (ConnectionType.IINACTIPC, "IINACT"),
        (ConnectionType.Internal, "Internal Parser"),
    ];

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

        var radios = new RadioButtonGroupNode { VerticalPadding = 6 };
        foreach (var (type, label) in Sources)
            radios.AddButton(label, () => Select(type));
        radios.SelectedOption = Sources.First(entry => entry.Type == Source).Label;
        AddNode(1, radios);
        AddDummy(5);

        act = Page(width - Indent - Spacing);
        act.AddNode([
            Line("Uses ACT on your PC. Pick this if you already run ACT for FFLogs."),
            Line("In ACT, open Plugins > OverlayPlugin WSServer."),
            Line("On Stream/Local overlay, click Start. Keep ACT running while you play."),
            new LabeledTextInputNode
            {
                LabelText = "WebSocket URL",
                Text = WebSocketUrl,
                OnInputReceived = text => ChangeUrl(text.ToString()),
                OnInputComplete = text => ChangeUrl(text.ToString()),
            },
            Guide("ACT setup guide", "https://overlayplugin.github.io/docs/setup/"),
        ]);
        iinact = Page(width - Indent - Spacing);
        iinactInstall = Line("Checking IINACT…");
        iinact.AddNode([
            Line("Runs inside the game and saves logs you can upload to FFLogs."),
            iinactInstall,
            Guide("Install IINACT", "https://www.iinact.com/installation/"),
        ]);
        local = Page(width - Indent - Spacing);
        local.AddNode([
            Line("Use the built-in parser to see your stats in game."),
            Line("Some numbers can differ from ACT or IINACT."),
            Line("It doesn't save logs for FFLogs."),
        ]);
        AddNode(1, [act, iinact, local]);
        AddDummy(5);
        status = new LabeledTextButtonNode
        {
            LabelWidth = 272,
            LabelText = "Connection unchecked",
            ButtonText = "Check connection",
            OnClick = CheckConnection,
        };
        bypassCheck = Check("Continue without a connection", false, value =>
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
            ConnectionType.IINACTIPC when ready => "IINACT is ready",
            ConnectionType.IINACTIPC => "IINACT isn't ready",
            _ when !valid => "Enter a ws:// or wss:// URL",
            _ when probe != null => "Checking connection…",
            _ when ready => "Connected",
            _ when probeResult == false => "Couldn't connect to ACT",
            _ => "Connection unchecked",
        };
        if (status.LabelText.ToString() != label)
        {
            status.LabelText = label;
            status.LabelTextColor = ColorHelper.GetColor(ready ? 46u : !valid || probeResult == false ? 14u : 8u);
        }

        if (Source == ConnectionType.IINACTIPC)
        {
            var plugin = Service.PluginInterface.InstalledPlugins.FirstOrDefault(entry => entry.InternalName == "IINACT");
            var installation = plugin is null ? "IINACT isn't installed."
                : !plugin.IsLoaded ? "IINACT is installed. Enable it in Dalamud."
                : ready ? "IINACT is enabled in Dalamud." : "IINACT is loaded. Check its settings.";
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
