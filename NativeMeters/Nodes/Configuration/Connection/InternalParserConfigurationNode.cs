using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Connection;

internal sealed class InternalParserConfigurationNode : TabbedVerticalListNode
{
    private const float RowWidth = 400.0f;
    private const float LabelWidth = 190.0f;

    public InternalParserConfigurationNode()
    {
        var config = System.Config.InternalParser;
        ItemSpacing = 2;
        FitWidth = true;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Internal Parser Options",
        });

        AddTab(1);

        AddNode([
            new LabeledEnumDropdownNode<ParseFilter>
            {
                Size = new Vector2(RowWidth, 28),
                LabelWidth = LabelWidth,
                LabelText = "Parse Filter",
                Options = Enum.GetValues<ParseFilter>().ToList(),
                SelectedOption = config.ParseFilter,
                OnOptionSelected = option =>
                {
                    config.ParseFilter = option;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = new Vector2(RowWidth, 20),
                String = "Combine Pets with Owner",
                IsChecked = config.MergePetDamage,
                TextTooltip = "When enabled, pet and summon damage is attributed to the owner.\n"
                              + "Disable to show pet damage as separate entries.",
                OnClick = isChecked =>
                {
                    config.MergePetDamage = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = new Vector2(RowWidth, 20),
                String = "Use \"YOU\" instead of your character's name",
                IsChecked = config.UseYouForLocalPlayer,
                TextTooltip = "When enabled, your character is named \"YOU\" to match ACT behavior.",
                OnClick = isChecked =>
                {
                    config.UseYouForLocalPlayer = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = new Vector2(RowWidth, 20),
                String = "Show Companions as Separate Entries",
                IsChecked = config.ShowCompanions,
                TextTooltip = "When enabled, damage from companions are shown as separate entries instead of being merged with the player.",
                OnClick = isChecked =>
                {
                    config.UseYouForLocalPlayer = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        SubtractTab(1);
        RecalculateLayout();
    }
}
