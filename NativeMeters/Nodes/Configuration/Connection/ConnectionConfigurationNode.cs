using System;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Input; // Assuming TextInputNode or similar lives here

namespace NativeMeters.Nodes.Configuration.Connection;

internal sealed class ConnectionConfigurationNode : TabbedVerticalListNode
{
    private readonly LabeledNumericInputNode reconnectIntervalSlider;

    public ConnectionConfigurationNode()
    {
        ConnectionSettings config = System.Config.ConnectionSettings;

        ItemVerticalSpacing = 5;

        // --- Section: Server Connection ---
        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Server Connection Settings",
        });

        AddTab(1);

        var typeDropDown = new LabeledDropdownNode
        {
            Size = new Vector2(300, 20),
            LabelText = "Connection Type",
            LabelTextFlags = TextFlags.AutoAdjustNodeSize,
            Options = Enum.GetNames(typeof(ConnectionType)).ToList(),
            SelectedOption = config.SelectedConnectionType.ToString(),
            OnOptionSelected = selected =>
            {
                if (Enum.TryParse<ConnectionType>(selected, out var parsed))
                {
                    config.SelectedConnectionType = parsed;
                }
            }
        };
        AddNode(typeDropDown);

        var urlInputNode = new LabeledTextInputNode
        {
            Size = new Vector2(300, 20),
            LabelText = "WebSocket URL",
            Text = config.WebSocketUrl,
            OnInputComplete = text =>
            {
                config.WebSocketUrl = text.ExtractText();
            }
        };
        AddNode(urlInputNode);

        SubtractTab(1);
        AddNode(new ResNode { Height = 10 });

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Reconnection & Logging",
        });

        AddTab(1);

        var autoReconnectCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Enable Auto-Reconnect",
            IsChecked = config.AutoReconnect,
            OnClick = isChecked =>
            {
                config.AutoReconnect = isChecked;
                if (reconnectIntervalSlider != null)
                    reconnectIntervalSlider.IsEnabled = isChecked;
            }
        };
        AddNode(autoReconnectCheckbox);

        reconnectIntervalSlider = new LabeledNumericInputNode
        {
            Size = new Vector2(300, 20),
            LabelText = "Reconnect Interval (s)",
            Min = 1,
            Max = 60,
            Value = config.AutoReconnectInterval,
            IsEnabled = config.AutoReconnect,
            OnValueUpdate = value =>
            {
                config.AutoReconnectInterval = value;
            }
        };
        AddNode(reconnectIntervalSlider);

        var logErrorsCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Log connection errors to chat/log",
            IsChecked = config.LogConnectionErrors,
            OnClick = isChecked =>
            {
                config.LogConnectionErrors = isChecked;
            }
        };
        AddNode(logErrorsCheckbox);

        SubtractTab(1);
    }
}