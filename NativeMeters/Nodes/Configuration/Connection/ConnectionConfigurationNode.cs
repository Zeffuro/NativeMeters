using System;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Connection;

internal sealed class ConnectionConfigurationNode : TabbedVerticalListNode
{
    private readonly LabeledNumericInputNode reconnectIntervalSlider;
    private readonly LabeledTextButtonNode statusNode;
    private readonly CircleButtonNode warningButton;
    private readonly LabeledTextInputNode urlInputNode;
    private readonly CircleButtonNode urlResetButton;

    private const string InternalWarningTooltip =
        "Internal Parser (Experimental)\n\n" +
        "• DoT/HoT damage is estimated, not simulated.\n" +
        "  Expect ~5-10% variance vs ACT/IINACT.\n" +
        "• DoT ticks are attributed by scanning active\n" +
        "  statuses, multi-DoT party accuracy is approximate.\n" +
        "• No damage shield or limit break tracking.\n\n" +
        "For accurate parsing, use ACT/IINACT instead.";

    public ConnectionConfigurationNode()
    {
        ConnectionSettings config = System.Config.ConnectionSettings;

        ItemVerticalSpacing = 2;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Server Connection Settings",
        });

        AddTab(1);

        statusNode = new LabeledTextButtonNode
        {
            Size = new Vector2(368, 28),
            OnClick = OnReconnectClicked,
            LabelText = "Status: Disconnected",
            ButtonText = "Reconnect",
        };
        AddNode(statusNode);

        var typeContainer = new SimpleComponentNode
        {
            Size = new Vector2(400, 28),
        };

        var typeDropDown = new LabeledEnumDropdownNode<ConnectionType>
        {
            Size = new Vector2(370, 28),
            LabelText = "Connection Type",
            LabelTextFlags = TextFlags.AutoAdjustNodeSize,
            Options = Enum.GetValues<ConnectionType>().ToList(),
            SelectedOption = config.SelectedConnectionType,
            OnOptionSelected = OnConnectionTypeSelected
        };
        typeDropDown.AttachNode(typeContainer);

        warningButton = new CircleButtonNode
        {
            Icon = ButtonIcon.Exclamation,
            Size = new Vector2(24f),
            Position = new Vector2(374, 2),
            IsVisible = config.SelectedConnectionType == ConnectionType.Internal,
            TextTooltip = InternalWarningTooltip,
        };
        warningButton.AttachNode(typeContainer);

        AddNode(typeContainer);

        var urlContainer = new SimpleComponentNode
        {
            Size = new Vector2(400, 28),
            IsVisible = config.SelectedConnectionType == ConnectionType.WebSocket,
        };

        urlInputNode = new LabeledTextInputNode
        {
            Size = new Vector2(370, 28),
            LabelText = "WebSocket URL",
            Text = config.WebSocketUrl,
            OnInputComplete = text =>
            {
                config.WebSocketUrl = text.ExtractText();
                OnReconnectClicked();
                ConfigRepository.Save(System.Config);
            }
        };
        urlInputNode.AttachNode(urlContainer);

        urlResetButton = new CircleButtonNode
        {
            Icon = ButtonIcon.Undo,
            Size = new Vector2(24f),
            Position = new Vector2(374, 2),
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
        urlResetButton.AttachNode(urlContainer);

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
            Size = new Vector2(400, 20),
            LabelText = "Reconnect Interval (s)",
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
        urlInputNode.IsVisible = selected == ConnectionType.WebSocket;
        urlResetButton.IsVisible = selected == ConnectionType.WebSocket;

        if (previousType == ConnectionType.Internal)
        {
            System.InternalMeterService.Dispose();
            System.InternalMeterService = new Services.Internal.InternalMeterService();
        }
        else
        {
            System.MeterService.Reconnect();
        }

        if (selected == ConnectionType.Internal)
        {
            System.InternalMeterService.Enable();
        }
        else
        {
            System.MeterService.Reconnect();
        }

        System.OverlayManager.UpdateActiveService();
        RecalculateLayout();
    }

    private void OnReconnectClicked()
    {
        if (System.Config.ConnectionSettings.SelectedConnectionType == ConnectionType.Internal)
        {
            System.InternalMeterService.Dispose();
            System.InternalMeterService = new Services.Internal.InternalMeterService();
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

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        base.Dispose(disposing, isNativeDestructor);
    }
}
