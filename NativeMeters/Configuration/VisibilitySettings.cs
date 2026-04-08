namespace NativeMeters.Configuration;

public class VisibilitySettings
{
    public bool HideWithNativeUi { get; set; } = true;

    // Hide conditions
    public bool HideOutsideOfCombat { get; set; } = false;
    public bool HideInCombat { get; set; } = false;
    public bool HideInGoldSaucer { get; set; } = false;
    public bool HideWhileFullHp { get; set; } = false;
    public bool HideWhenInDuty { get; set; } = false;
    public bool HideInSanctuary { get; set; } = false;
    public bool HideInPvP { get; set; } = false;
    public bool HideWhilePerforming { get; set; } = false;
    public bool HideInCutscene { get; set; } = false;
    public bool HideWhenCrafting { get; set; } = false;
    public bool HideWhenGathering { get; set; } = false;

    public bool AlwaysShowWhenInDuty { get; set; } = false;
    public bool AlwaysShowWhenWeaponDrawn { get; set; } = false;
    public bool AlwaysShowWhileInParty { get; set; } = false;
    public bool AlwaysShowWhileInPvP { get; set; } = false;
    public bool AlwaysShowWhileTargetExists { get; set; } = false;

    public bool HasAnyCondition =>
        HideOutsideOfCombat || HideInCombat || HideInGoldSaucer || HideWhileFullHp ||
        HideWhenInDuty || HideInSanctuary || HideInPvP || HideWhilePerforming || HideInCutscene ||
        HideWhenCrafting || HideWhenGathering ||
        AlwaysShowWhenInDuty || AlwaysShowWhenWeaponDrawn ||
        AlwaysShowWhileInParty || AlwaysShowWhileInPvP || AlwaysShowWhileTargetExists;
}
