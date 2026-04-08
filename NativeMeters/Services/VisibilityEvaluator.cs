using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using NativeMeters.Configuration;
using NativeMeters.Extensions;

namespace NativeMeters.Services;

public static unsafe class VisibilityEvaluator
{
    private static readonly uint[] GoldSaucerTerritoryIds = [144, 388, 389, 390, 391, 579, 792, 899, 941];

    public static bool ShouldHide()
    {
        var settings = System.Config.Visibility;
        if (!settings.HasAnyCondition) return false;
        if (CheckAlwaysShowConditions(settings)) return false;
        return CheckHideConditions(settings);
    }

    private static bool CheckAlwaysShowConditions(VisibilitySettings settings)
    {
        var condition = Service.Condition;

        if (settings.AlwaysShowWhenInDuty && condition.IsBoundByDuty)
            return true;

        if (settings.AlwaysShowWhenWeaponDrawn && IsWeaponDrawn())
            return true;

        if (settings.AlwaysShowWhileInParty && Service.PartyList.Length > 0)
            return true;

        if (settings.AlwaysShowWhileInPvP && Service.ClientState.IsPvPExcludingDen)
            return true;

        if (settings.AlwaysShowWhileTargetExists && Service.TargetManager.Target != null)
            return true;

        return false;
    }

    private static bool CheckHideConditions(VisibilitySettings settings)
    {
        var condition = Service.Condition;

        if (settings.HideOutsideOfCombat && !condition.IsInCombat)
            return true;

        if (settings.HideInCombat && condition.IsInCombat)
            return true;

        if (settings.HideInGoldSaucer && GoldSaucerTerritoryIds.Contains(Service.ClientState.TerritoryType))
            return true;

        if (settings.HideWhileFullHp && IsAtFullHp())
            return true;

        if (settings.HideWhenInDuty && condition.IsBoundByDuty)
            return true;

        if (settings.HideInSanctuary && TerritoryInfo.Instance()->InSanctuary)
            return true;

        if (settings.HideInPvP && Service.ClientState.IsPvPExcludingDen)
            return true;

        if (settings.HideWhilePerforming && condition.IsPerforming)
            return true;

        if (settings.HideInCutscene && condition.IsInCutsceneOrQuestEvent)
            return true;

        if (settings.HideWhenCrafting && condition.IsCrafting)
            return true;

        if (settings.HideWhenGathering && condition.IsGathering)
            return true;

        return false;
    }

    private static bool IsAtFullHp()
    {
        var player = Service.ObjectTable.LocalPlayer;
        return player != null && player.CurrentHp >= player.MaxHp;
    }

    private static bool IsWeaponDrawn()
    {
        var player = Service.ObjectTable.LocalPlayer;
        return player != null && player.StatusFlags.HasFlag(StatusFlags.WeaponOut);
    }
}
