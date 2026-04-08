using KamiToolKit.Nodes;
using KamiToolKit.Premade.Node;
using NativeMeters.Configuration.Persistence;

namespace NativeMeters.Nodes.Configuration.Visibility;

internal sealed class VisibilityConfigurationNode : TabbedVerticalListNode
{
    public VisibilityConfigurationNode()
    {
        var config = System.Config.Visibility;

        ItemVerticalSpacing = 2;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "General",
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = "Hide with Native UI",
            IsChecked = config.HideWithNativeUi,
            OnClick = isChecked =>
            {
                config.HideWithNativeUi = isChecked;
                ConfigRepository.Save(System.Config);
            }
        });

        AddNode(new ResNode { Height = 10 });

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Hide Conditions",
        });

        AddNode(1, [
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide outside of combat",
                IsChecked = config.HideOutsideOfCombat,
                OnClick = isChecked =>
                {
                    config.HideOutsideOfCombat = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide in combat",
                IsChecked = config.HideInCombat,
                OnClick = isChecked =>
                {
                    config.HideInCombat = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide in Gold Saucer",
                IsChecked = config.HideInGoldSaucer,
                OnClick = isChecked =>
                {
                    config.HideInGoldSaucer = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide while at full HP",
                IsChecked = config.HideWhileFullHp,
                OnClick = isChecked =>
                {
                    config.HideWhileFullHp = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide when in duty",
                IsChecked = config.HideWhenInDuty,
                OnClick = isChecked =>
                {
                    config.HideWhenInDuty = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide in Sanctuary",
                IsChecked = config.HideInSanctuary,
                OnClick = isChecked =>
                {
                    config.HideInSanctuary = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide in PvP",
                IsChecked = config.HideInPvP,
                OnClick = isChecked =>
                {
                    config.HideInPvP = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide while performing",
                IsChecked = config.HideWhilePerforming,
                OnClick = isChecked =>
                {
                    config.HideWhilePerforming = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide in cutscene / quest event",
                IsChecked = config.HideInCutscene,
                OnClick = isChecked =>
                {
                    config.HideInCutscene = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide when crafting",
                IsChecked = config.HideWhenCrafting,
                OnClick = isChecked =>
                {
                    config.HideWhenCrafting = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Hide when gathering",
                IsChecked = config.HideWhenGathering,
                OnClick = isChecked =>
                {
                    config.HideWhenGathering = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        AddNode(new ResNode { Height = 10 });

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = "Always Show Conditions",
        });

        AddNode(1, [
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Always show when in duty",
                IsChecked = config.AlwaysShowWhenInDuty,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhenInDuty = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Always show when weapon is drawn",
                IsChecked = config.AlwaysShowWhenWeaponDrawn,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhenWeaponDrawn = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Always show while in a party",
                IsChecked = config.AlwaysShowWhileInParty,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhileInParty = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Always show while in PvP",
                IsChecked = config.AlwaysShowWhileInPvP,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhileInPvP = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = "Always show while target exists",
                IsChecked = config.AlwaysShowWhileTargetExists,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhileTargetExists = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        SubtractTab(1);
    }
}
