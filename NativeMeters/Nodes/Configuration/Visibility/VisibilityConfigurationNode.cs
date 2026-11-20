using KamiToolKit.Nodes;
using NativeMeters.Localization;
using NativeMeters.Configuration.Persistence;
using CheckboxNode = NativeMeters.Nodes.Input.SyncedCheckboxNode;

namespace NativeMeters.Nodes.Configuration.Visibility;

internal sealed class VisibilityConfigurationNode : TabbedVerticalListNode
{
    public VisibilityConfigurationNode()
    {
        var config = System.Config.Visibility;

        ItemSpacing = 2;
        FitWidth = true;

        AddNode(new CategoryTextNode
        {
            Height = 18,
            String = Loc.Get("VisibilityGeneral"),
        });

        AddNode(1, new CheckboxNode
        {
            Size = Size with { Y = 18 },
            IsVisible = true,
            String = Loc.Get("VisibilityWithHud"),
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
            String = Loc.Get("VisibilityHideConditions"),
        });

        AddNode(1, [
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = Loc.Get("VisibilityOutOfCombat"),
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
                String = Loc.Get("VisibilityInCombat"),
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
                String = Loc.Get("VisibilityGoldSaucer"),
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
                String = Loc.Get("VisibilityFullHp"),
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
                String = Loc.Get("VisibilityInDuty"),
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
                String = Loc.Get("VisibilitySanctuary"),
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
                String = Loc.Get("VisibilityPvp"),
                IsChecked = config.HideInPvP,
                TextTooltip = Loc.Get("VisibilityPvpHelp"),
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
                String = Loc.Get("VisibilityPerforming"),
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
                String = Loc.Get("VisibilityCutscene"),
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
                String = Loc.Get("VisibilityCrafting"),
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
                String = Loc.Get("VisibilityGathering"),
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
            String = Loc.Get("VisibilityAlwaysConditions"),
        });

        AddNode(1, [
            new CheckboxNode
            {
                Size = Size with { Y = 18 },
                IsVisible = true,
                String = Loc.Get("VisibilityAlwaysDuty"),
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
                String = Loc.Get("VisibilityAlwaysDrawn"),
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
                String = Loc.Get("VisibilityAlwaysParty"),
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
                String = Loc.Get("VisibilityAlwaysPvp"),
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
                String = Loc.Get("VisibilityAlwaysTarget"),
                IsChecked = config.AlwaysShowWhileTargetExists,
                OnClick = isChecked =>
                {
                    config.AlwaysShowWhileTargetExists = isChecked;
                    ConfigRepository.Save(System.Config);
                }
            },
        ]);

        SubtractTab(1);
        RecalculateLayout();
    }
}
