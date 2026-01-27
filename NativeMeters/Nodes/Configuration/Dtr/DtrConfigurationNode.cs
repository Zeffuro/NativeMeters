using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Dtr;

public sealed class DtrConfigurationNode : TabbedVerticalListNode
{
    private readonly LabeledDropdownNode tagDropdown;

    public DtrConfigurationNode()
    {
        var settings = System.Config.DtrSettings;
        ItemVerticalSpacing = 2;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Server Info Bar",
        });

        var enableCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            String = "Enabled",
            IsChecked = settings.Enabled,
            OnClick = isChecked =>
            {
                settings.Enabled = isChecked;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, enableCheckbox);
        AddNode(new ResNode { Height = 10 });

        var formatInput = new LabeledTextInputNode
        {
            LabelText = "Format String:",
            Size = new Vector2(400, 28),
            Text = settings.FormatString,
            Placeholder = "[dps:k.1] DPS",
            TextTooltip = "Format syntax: [tag_part: modifier.precision]\n" +
                          "• : r = Raw (no commas)\n" +
                          "• :k/: m = Kilo/Mega units\n" +
                          "• .N = Decimals\n" +
                          "Example: [dps:k.1] -> 12.3k",
            OnInputComplete = text =>
            {
                settings.FormatString = text.ExtractText();
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, formatInput);

        tagDropdown = new LabeledDropdownNode
        {
            LabelText = "Insert Tag:",
            Size = new Vector2(400, 28),
            Options = TagDefinitions.GetLabels(),
            MaxListOptions = 10,
            SelectedOption = TagDefinitions.DefaultDropdownLabel,
            OnOptionSelected = selected =>
            {
                if (TagDefinitions.Templates.TryGetValue(selected, out var tag) && !string.IsNullOrEmpty(tag))
                {
                    formatInput.Text += tag;

                    settings.FormatString = formatInput.Text.ExtractText();
                    ConfigRepository.Save(System.Config);

                    tagDropdown?.SelectedOption = TagDefinitions.DefaultDropdownLabel;
                }
            }
        };
        AddNode(1, tagDropdown);
        AddNode(new ResNode { Height = 10 });

        var showDisconnectedCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            String = "Show when disconnected",
            IsChecked = settings.ShowWhenDisconnected,
            OnClick = isChecked =>
            {
                settings.ShowWhenDisconnected = isChecked;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, showDisconnectedCheckbox);

        var disconnectTextInput = new LabeledTextInputNode
        {
            LabelText = "Disconnect Text:",
            Size = new Vector2(400, 28),
            Text = settings.DisconnectedText,
            OnInputComplete = text =>
            {
                settings.DisconnectedText = text.ExtractText();
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, disconnectTextInput);
        AddNode(new ResNode { Height = 10 });

        var clickToOpenCheckbox = new CheckboxNode
        {
            Size = Size with { Y = 20 },
            String = "Click to open config window",
            IsChecked = settings.ClickToOpenConfig,
            OnClick = isChecked =>
            {
                settings.ClickToOpenConfig = isChecked;
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, clickToOpenCheckbox);

        SubtractTab(1);
    }
}
