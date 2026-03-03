using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using NativeMeters.Services;

namespace NativeMeters.Utilities;

public static class EnmityUtilities
{
    public static void ResetAllStrikingDummies()
    {
        foreach (var obj in Service.ObjectTable)
        {
            if (obj is IBattleChara { NameId: 541 } dummy)
            {
                bool isInCombat = Service.Condition[ConditionFlag.InCombat];
                if (!isInCombat) continue;

                GameMain.ExecuteCommand(319, (int)dummy.GameObjectId);
            }
        }
    }
}
