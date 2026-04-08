namespace NativeMeters.Models.Internal;

public enum ActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    FullResist = 2,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
    Invulnerable = 7,
    NoEffect = 8,
    Unknown9 = 9,
    MpLoss = 10,
    MpGain = 11,
    TpLoss = 12,
    GpGain = 13, // Used to be TpGain but eventually got replaced because Tp is no longer a thing

    ApplyStatusTarget = 14,
    ApplyStatusSource = 15,
    RecoveredStatus = 16,
    LoseStatusTarget = 17,
    LoseStatusSource = 18,
    StatusNoEffect = 20,

    EnminityIndex = 24, // Verified, adds LogMessage 536
    EnmityAmountUp = 25, // Verified ^
    EnmityAmountDown = 26, // Unsure but makes sense with the position
    StartActionCombo = 27,
    ComboStep = 28,

    Knockback = 31,
    Attract = 32,
    Attract2 = 33,
    Dash = 34,
    Dash2 = 35,
    Dash3 = 36,

    MountSfx = 39,

    StatusDispel1 = 47,
    StatusDispel2 = 48,
    StatusDispel3 = 49,

    InstantDeath = 50, // Some sources mention this is Revive LB, triggers LogMessage 519
    InstantDeath2 = 51, // Triggers LogMessage 519,

    FullResistStatus = 55, // Trigger LogMessage 596
    Vulnerability = 57, // Triggers LogMessage 456, "been sentenced to death!"

    SxtBattleLogMessage = 60,
    ActionChange = 61, // Some sources say this is JobGauge, unsure.
    Unknown62 = 62, // Some sources mention this is Gaining WAR Gauge
    ToggleVis = 65,
    SetModelScale = 68,

    SetHp = 74, // Some sources mention this is stuff like Zodiark's Kokytos
    PartialInvulnerable = 75,
    Interrupt = 76,
}
