using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Localization;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;
using NativeMeters.Services;
using CheckboxNode = NativeMeters.Nodes.Input.SyncedCheckboxNode;

namespace NativeMeters.Nodes.Configuration.Connection;

internal sealed class ConnectionConfigurationNode : TabbedVerticalListNode
{
    private const float RowWidth = 400.0f;
    private const float LabelWidth = 190.0f;
    private const float RowHeight = 28.0f;
    private const float InlineButtonSize = 24.0f;
    private const float InlineSpacing = 4.0f;

    private readonly LabeledNumericInputNode reconnectIntervalSlider;
    private readonly LabeledTextButtonNode statusNode;
    private readonly CircleButtonNode warningButton;
    private readonly HorizontalListNode urlContainer;
    private readonly LabeledTextInputNode urlInputNode;
    private readonly CircleButtonNode urlResetButton;
    private bool? lastConnectionState;
    private long lastStatusCheck;

    public Action<ConnectionType>? OnConnectionTypeChanged { get; set; }

    public ConnectionConfigurationNode()
    {
        ConnectionSettings config = System.Config.ConnectionSettings;

        ItemSpacing = 2;
        FitWidth = true;


        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = Loc.Get("ConnectionSettings"),
        });

        AddTab(1);

        statusNode = new LabeledTextButtonNode
        {
            Size = new Vector2(RowWidth, RowHeight),
            LabelWidth = LabelWidth,
            OnClick = OnReconnectClicked,
            LabelText = Loc.Get("ConnectionDisconnected"),
            ButtonText = Loc.Get("ConnectionReconnect"),
            NavUp = 2,
        };
        AddNode(statusNode);

        var typeContainer = new HorizontalListNode
        {
            Size = new Vector2(RowWidth + InlineButtonSize + InlineSpacing, RowHeight),
            ItemSpacing = InlineSpacing,
        };

        var typeDropDown = new LabeledEnumDropdownNode<ConnectionType>
        {
            Size = new Vector2(RowWidth, RowHeight),
            LabelWidth = LabelWidth,
            LabelText = Loc.Get("ConnectionTypeLabel"),
            Options = Enum.GetValues<ConnectionType>().ToList(),
            SelectedOption = config.SelectedConnectionType,
            OnOptionSelected = OnConnectionTypeSelected
        };
        typeContainer.AddNode(typeDropDown);

        warningButton = new CircleButtonNode
        {
            Icon = CircleButtonIcon.Exclamation,
            Size = new Vector2(InlineButtonSize),
            IsVisible = config.SelectedConnectionType == ConnectionType.Internal,
            TextTooltip = Loc.Get("ConnectionParserWarning"),
        };
        typeContainer.AddNode(warningButton);

        AddNode(typeContainer);

        urlContainer = new HorizontalListNode
        {
            Size = new Vector2(RowWidth + InlineButtonSize + InlineSpacing, RowHeight),
            ItemSpacing = InlineSpacing,
            IsVisible = config.SelectedConnectionType == ConnectionType.WebSocket,
        };

        urlInputNode = new LabeledTextInputNode
        {
            Size = new Vector2(RowWidth, RowHeight),
            LabelWidth = LabelWidth,
            LabelText = Loc.Get("ConnectionUrl"),
            Text = config.WebSocketUrl,
            OnInputComplete = text =>
            {
                config.WebSocketUrl = text.ToString();
                OnReconnectClicked();
                ConfigRepository.Save(System.Config);
            }
        };
        urlContainer.AddNode(urlInputNode);

        urlResetButton = new CircleButtonNode
        {
            Icon = CircleButtonIcon.Undo,
            Size = new Vector2(InlineButtonSize),
            TextTooltip = Loc.Get("ConnectionResetUrl"),
            OnClick = () =>
            {
                var defaultUrl = new ConnectionSettings().WebSocketUrl;
                config.WebSocketUrl = defaultUrl;
                urlInputNode.Text = defaultUrl;
                OnReconnectClicked();
                ConfigRepository.Save(System.Config);
            }
        };
        urlContainer.AddNode(urlResetButton);

        AddNode(urlContainer);

        SubtractTab(1);
        AddNode(new ResNode { Height = 10 });

        AddNode(new CategoryTextNode
        {
            Height = 28,
            String = Loc.Get("ConnectionLogging"),
        });

        AddTab(1);

        var autoReconnectCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            IsVisible = true,
            String = Loc.Get("ConnectionAutoReconnect"),
            IsChecked = config.AutoReconnect,
            OnClick = isChecked =>
            {
                config.AutoReconnect = isChecked;
                reconnectIntervalSlider?.IsEnabled = isChecked;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(autoReconnectCheckbox);

        reconnectIntervalSlider = new LabeledNumericInputNode
        {
            Size = new Vector2(RowWidth, RowHeight),
            LabelWidth = LabelWidth,
            LabelText = Loc.Get("ConnectionInterval"),
            Min = 1,
            Max = 60,
            Value = config.AutoReconnectInterval,
            IsEnabled = config.AutoReconnect,
            OnValueUpdate = value =>
            {
                config.AutoReconnectInterval = value;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(reconnectIntervalSlider);

        var logErrorsCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            IsVisible = true,
            String = Loc.Get("ConnectionLogErrors"),
            IsChecked = config.LogConnectionErrors,
            OnClick = isChecked =>
            {
                config.LogConnectionErrors = isChecked;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(logErrorsCheckbox);

        SubtractTab(1);

        Service.Framework.Update += OnFrameworkUpdate;
    }

    private void OnConnectionTypeSelected(ConnectionType selected)
    {
        ConnectionSelection.Apply(selected);
        ConfigRepository.Save(System.Config);

        warningButton.IsVisible = selected == ConnectionType.Internal;
        urlContainer.IsVisible = selected == ConnectionType.WebSocket;
        urlInputNode.IsVisible = selected == ConnectionType.WebSocket;
        urlResetButton.IsVisible = selected == ConnectionType.WebSocket;

        OnConnectionTypeChanged?.Invoke(selected);
        RecalculateLayout();
    }

    private void OnReconnectClicked()
    {
        if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
        {
            System.InternalMeterService.Dispose();
            System.InternalMeterService = new Services.Internal.InternalMeterService();
            System.AddonDetailedBreakdownWindow.ReSubscribeToEvents();
            System.InternalMeterService.Enable();
            System.OverlayManager.UpdateActiveService();
        }
        else
        {
            System.MeterService.Reconnect();
        }
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (System.ActiveMeterService is null)
            return;

        var timestamp = Stopwatch.GetTimestamp();
        if (lastConnectionState.HasValue && Stopwatch.GetElapsedTime(lastStatusCheck, timestamp).TotalMilliseconds < 250)
            return;

        lastStatusCheck = timestamp;
        bool isConnected = System.ActiveMeterService.IsConnected;
        if (lastConnectionState == isConnected)
            return;

        lastConnectionState = isConnected;

        statusNode.LabelText = isConnected ? Loc.Get("ConnectionConnected") : Loc.Get("ConnectionDisconnected");
        statusNode.LabelTextColor = isConnected
            ? ColorHelper.GetColor(46)
            : ColorHelper.GetColor(14);
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        if (Service.Framework is not null)
        {
            Service.Framework.Update -= OnFrameworkUpdate;
        }

        base.Dispose(isNativeDestructor);
    }
}
