using System;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;
using CheckboxNode = NativeMeters.Nodes.Input.SyncedCheckboxNode;

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
            String = Loc.Get("ConnectionParserOptions"),
        });

        AddTab(1);

        AddNode([
            new LabeledEnumDropdownNode<ParseFilter>
            {
                Size = new Vector2(RowWidth, 28),
                LabelWidth = LabelWidth,
                LabelText = Loc.Get("ConnectionParseFilter"),
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
                String = Loc.Get("ConnectionMergePets"),
                IsChecked = config.MergePetDamage,
                TextTooltip = Loc.Get("ConnectionMergePetsHelp"),
                OnClick = isChecked =>
                {
                    config.MergePetDamage = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = new Vector2(RowWidth, 20),
                String = Loc.Get("ConnectionUseYou"),
                IsChecked = config.UseYouForLocalPlayer,
                TextTooltip = Loc.Get("ConnectionUseYouHelp"),
                OnClick = isChecked =>
                {
                    config.UseYouForLocalPlayer = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = new Vector2(RowWidth, 20),
                String = Loc.Get("ConnectionSeparateCompanions"),
                IsChecked = config.ShowCompanions,
                TextTooltip = Loc.Get("ConnectionSeparateCompanionsHelp"),
                OnClick = isChecked =>
                {
                    config.ShowCompanions = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        SubtractTab(1);
        RecalculateLayout();
    }
}
