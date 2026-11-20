using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Dtr;

public sealed class DtrConfigurationNode : TabbedVerticalListNode
{
    private const float RowHeight = 28.0f;
    private const float HelpButtonSize = 24.0f;

    private readonly LabeledDropdownNode tagDropdown;
    private readonly HorizontalListNode formatRow;
    private readonly LabeledTextInputNode formatInput;
    private readonly CircleButtonNode formatHelpButton;

    public DtrConfigurationNode()
    {
        var settings = System.Config.DtrSettings;
        ItemSpacing = 2;

        AddNode(new CategoryTextNode
        {
            TextFlags = TextFlags.Ellipsis,
            String = Loc.Get("DtrHeading"),
            Height = 18,
        });

        var enableCheckbox = new CheckboxRowNode
        {
            Height = 20,
            String = Loc.Get("GeneralEnabled"),
            IsChecked = settings.Enabled,
            OnClick = isChecked =>
            {
                settings.Enabled = isChecked;
                System.DtrService?.UpdateBar();
                ConfigRepository.Save(System.Config);
            }
        };
        AddNode(1, enableCheckbox);
        AddNode(1, new CheckboxRowNode
        {
            Height = 20,
            String = Loc.Get("DtrPersonalDps"),
            TextTooltip = Loc.Get("DtrPersonalDpsHelp"),
            IsChecked = settings.UsePersonalDps,
            OnClick = isChecked =>
            {
                settings.UsePersonalDps = isChecked;
                System.DtrService?.UpdateBar();
                ConfigRepository.Save(System.Config);
            }
        });
        AddNode(new ResNode { Height = 10 });

        formatRow = new HorizontalListNode
        {
            Size = new Vector2(400, RowHeight),
            ItemSpacing = 4.0f,
        };

        formatInput = new LabeledTextInputNode
        {
            LabelText = Loc.Get("DtrFormat"),
            Size = new Vector2(400, RowHeight),
            Text = settings.FormatString,
            Placeholder = "[dps:k.1] DPS",
            OnInputComplete = text =>
            {
                settings.FormatString = text.ToString();
                ConfigRepository.Save(System.Config);
            }
        };

        formatHelpButton = new CircleButtonNode
        {
            Icon = CircleButtonIcon.QuestionMark,
            Size = new Vector2(HelpButtonSize),
            Y = (RowHeight - HelpButtonSize) / 2.0f,
            TextTooltip = TagFormatHelp.BasicTooltip,
        };

        formatRow.AddNode([formatInput, formatHelpButton]);
        AddNode(1, formatRow);

        tagDropdown = new LabeledDropdownNode
        {
            LabelText = Loc.Get("DtrInsertTag"),
            Size = new Vector2(400, 28),
            GetOptionLabel = Loc.TagLabel,
            Options = TagDefinitions.GetLabels(),
            MaxListOptions = 10,
            SelectedOption = TagDefinitions.DefaultDropdownLabel,
            OnOptionSelected = selected =>
            {
                if (TagDefinitions.Templates.TryGetValue(selected, out var tag) && !string.IsNullOrEmpty(tag))
                {
                    formatInput.Text += tag;

                    settings.FormatString = formatInput.Text.ToString();
                    ConfigRepository.Save(System.Config);

                    tagDropdown?.SelectedOption = TagDefinitions.DefaultDropdownLabel;
                }
            }
        };
        AddNode(1, tagDropdown);
        AddNode(new ResNode { Height = 10 });

        AddNode(1, [
            new CheckboxRowNode
            {
                Height = 20,
                String = Loc.Get("DtrShowDisconnected"),
                IsChecked = settings.ShowWhenDisconnected,
                OnClick = isChecked =>
                {
                    settings.ShowWhenDisconnected = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new LabeledTextInputNode
            {
                LabelText = Loc.Get("DtrDisconnectedText"),
                Size = new Vector2(400, 28),
                Text = settings.DisconnectedText,
                OnInputComplete = text =>
                {
                    settings.DisconnectedText = text.ToString();
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        AddNode(new ResNode { Height = 10 });

        AddNode(1, new CheckboxRowNode
        {
            Height = 20,
            String = Loc.Get("DtrClickConfig"),
            IsChecked = settings.ClickToOpenConfig,
            OnClick = isChecked =>
            {
                settings.ClickToOpenConfig = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        RecalculateLayout();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        FitWidth = Width > TabSize + ItemSpacing;
        if (FitWidth) UpdateFormatRowLayout();
    }

    private void UpdateFormatRowLayout()
    {
        if (formatRow is null || formatInput is null || formatHelpButton is null)
        {
            return;
        }

        var rowWidth = Math.Max(0.0f, Width - TabSize - ItemSpacing);

        var maxInputWidth = Math.Max(0.0f, rowWidth - HelpButtonSize - formatRow.ItemSpacing);
        var preferredInputWidth = formatInput.LabelWidth + formatInput.ControlSpacing + formatInput.MaximumControlWidth;

        formatRow.Size = new Vector2(rowWidth, RowHeight);
        formatHelpButton.Size = new Vector2(HelpButtonSize);
        formatHelpButton.Y = (RowHeight - HelpButtonSize) / 2.0f;
        formatInput.Size = new Vector2(Math.Min(maxInputWidth, preferredInputWidth), RowHeight);
        formatRow.RecalculateLayout();
    }
}
