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
    MpLoss = 10,
    MpGain = 11,
    TpLoss = 12,
    TpGain = 13,
    ApplyStatusTarget = 14,
    ApplyStatusSource = 15,
    RecoveredStatus = 16,
    LoseStatusTarget = 17,
    LoseStatusSource = 18,
    StatusNoEffect = 20,
    StartActionCombo = 27,
    Knockback = 33,
    Mount = 40,
}
