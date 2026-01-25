using Dalamud.Game.ClientState.Conditions;

namespace NativeMeters.Services;

public class CombatStateTracker
{
    private bool wasInCombat;

    public bool CheckCombatEnded()
    {
        if (!System.Config.General.ForceEndEncounter) return false;

        bool isInCombat = Service.Condition[ConditionFlag.InCombat];
        bool combatEnded = wasInCombat && !isInCombat;
        wasInCombat = isInCombat;

        return combatEnded;
    }
}
