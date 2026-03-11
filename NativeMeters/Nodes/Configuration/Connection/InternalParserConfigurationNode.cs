using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Data.Stats;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Connection;

internal sealed class InternalParserConfigurationNode : TabbedVerticalListNode
{
    public InternalParserConfigurationNode()
    {
        var config = System.Config.InternalParser;
        ItemVerticalSpacing = 2;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Internal Parser Options",
        });

        AddTab(1);

        AddNode(new LabeledEnumDropdownNode<ParseFilter>
        {
            Size = new Vector2(400, 28),
            LabelText = "Parse Filter",
            Options = Enum.GetValues<ParseFilter>().ToList(),
            SelectedOption = config.ParseFilter,
            OnOptionSelected = option =>
            {
                config.ParseFilter = option;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new CheckboxNode
        {
            Size = new Vector2(400, 20),
            String = "Combine Pets with Owner",
            IsChecked = config.MergePetDamage,
            TextTooltip = "When enabled, pet and summon damage is attributed to the owner.\n"
                          + "Disable to show pet damage as separate entries.",
            OnClick = isChecked =>
            {
                config.MergePetDamage = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new CheckboxNode
        {
            Size = new Vector2(400, 20),
            String = "Use \"YOU\" instead of your character's name",
            IsChecked = config.UseYouForLocalPlayer,
            TextTooltip = "When enabled, your character is named \"YOU\" to match ACT behavior.",
            OnClick = isChecked =>
            {
                config.UseYouForLocalPlayer = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new CheckboxNode
        {
            Size = new Vector2(400, 20),
            String = "Show Companions as Separate Entries",
            IsChecked = config.ShowCompanions,
            TextTooltip = "When enabled, damage from companions are shown as separate entries instead of being merged with the player.",
            OnClick = isChecked =>
            {
                config.UseYouForLocalPlayer = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        SubtractTab(1);
    }
}
