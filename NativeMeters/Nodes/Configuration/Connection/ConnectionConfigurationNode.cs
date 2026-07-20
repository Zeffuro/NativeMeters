using System;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.Simplified;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;
using NativeMeters.Services;

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

    public Action<ConnectionType>? OnConnectionTypeChanged { get; set; }

    private const string InternalWarningTooltip =
        "Internal Parser (Experimental)\n\n" +
        "• DoT/HoT damage is estimated, not simulated.\n" +
        "  Expect ~5-10% variance vs ACT/IINACT.\n" +
        "• DoT ticks are attributed by scanning active\n" +
        "  statuses, multi-DoT party accuracy is approximate.\n" +
        "For accurate parsing, use ACT/IINACT instead.";

    public ConnectionConfigurationNode()
    {
        ConnectionSettings config = System.Config.ConnectionSettings;

        ItemSpacing = 2;
        FitWidth = true;


        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Server Connection Settings",
        });

        AddTab(1);

        statusNode = new LabeledTextButtonNode
        {
            Size = new Vector2(RowWidth, RowHeight),
            LabelWidth = LabelWidth,
            OnClick = OnReconnectClicked,
            LabelText = "Status: Disconnected",
            ButtonText = "Reconnect",
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
            LabelText = "Connection Type",
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
            TextTooltip = InternalWarningTooltip,
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
            LabelText = "WebSocket URL",
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
            TextTooltip = "Reset to default URL",
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
            String = "Reconnection & Logging",
        });

        AddTab(1);

        var autoReconnectCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            IsVisible = true,
            String = "Enable Auto-Reconnect",
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
            LabelText = "Reconnect Interval",
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
            String = "Log connection errors to log",
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
        var previousType = System.Config.ConnectionSettings.SelectedConnectionType;
        System.Config.ConnectionSettings.SelectedConnectionType = selected;
        ConfigRepository.Save(System.Config);

        warningButton.IsVisible = selected == ConnectionType.Internal;
        urlContainer.IsVisible = selected == ConnectionType.WebSocket;
        urlInputNode.IsVisible = selected == ConnectionType.WebSocket;
        urlResetButton.IsVisible = selected == ConnectionType.WebSocket;

        if (previousType == ConnectionType.Internal)
        {
            System.InternalMeterService.Dispose();
            System.InternalMeterService = new Services.Internal.InternalMeterService();
        }

        if (selected == ConnectionType.Internal || System.Config.General.EnableInternalParserForBreakdown)
        {
            System.InternalMeterService.Enable();
        }

        if (selected != ConnectionType.Internal)
        {
            System.MeterService.Reconnect();
        }

        OnConnectionTypeChanged?.Invoke(selected);
        System.OverlayManager.UpdateActiveService();
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
        bool isConnected = System.ActiveMeterService.IsConnected;

        statusNode.LabelText = isConnected ? "Status: Connected" : "Status: Disconnected";
        statusNode.LabelTextColor = isConnected
            ? ColorHelper.GetColor(46)
            : ColorHelper.GetColor(14);
    }

    protected override void Dispose(bool isNativeDestructor)
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        base.Dispose(isNativeDestructor);
    }
}
