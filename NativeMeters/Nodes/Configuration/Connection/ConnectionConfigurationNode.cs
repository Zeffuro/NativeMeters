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

    public ConnectionConfigurationNode()
    {
        ConnectionSettings config = System.Config.ConnectionSettings;

        ItemVerticalSpacing = 5;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Server Connection Settings",
        });

        AddTab(1);

        statusNode = new LabeledTextButtonNode
        {
            Size = new Vector2(400, 28),
            OnClick = () => System.MeterService.Reconnect(),
            LabelText= "Status: Disconnected",
            ButtonText = "Reconnect",
        };

        AddNode(statusNode);

        var typeDropDown = new LabeledEnumDropdownNode<ConnectionType>
        {
            Size = new Vector2(400, 28),
            LabelText = "Connection Type",
            LabelTextFlags = TextFlags.AutoAdjustNodeSize,
            Options = Enum.GetValues<ConnectionType>().ToList(),
            SelectedOption = config.SelectedConnectionType,
            OnOptionSelected = selected =>
            {
                config.SelectedConnectionType = selected;
                System.MeterService.Reconnect();
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(typeDropDown);

        var urlInputNode = new LabeledTextInputNode
        {
            Size = new Vector2(400, 28),
            LabelText = "WebSocket URL",
            Text = config.WebSocketUrl,
            OnInputComplete = text =>
            {
                config.WebSocketUrl = text.ExtractText();
                System.MeterService.Reconnect();
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(urlInputNode);

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

    private void OnFrameworkUpdate(IFramework framework)
    {
        bool isConnected = System.MeterService.IsConnected;

        statusNode.LabelText = isConnected ? "Status: Connected" : "Status: Disconnected";
        statusNode.LabelTextColor = isConnected
            ? ColorHelper.GetColor(46) // Green
            : ColorHelper.GetColor(14); // Red
    }

    protected override void Dispose(bool disposing, bool isNativeDestructor)
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        base.Dispose(disposing, isNativeDestructor);
    }
}
