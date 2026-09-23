using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using NativeMeters.Models.Internal;

namespace NativeMeters.Services.Internal;

public unsafe partial class NetworkCombatParser
{
    private void ActionEffectDetour(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos,
        ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        var receivedAt = DateTime.UtcNow;
        actionEffectHook!.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);

        try
        {
            if (header == null || casterPtr == null || effects == null || targetEntityIds == null ||
                header->NumTargets == 0) return;

            ulong resolvedSourceId = casterEntityId;
            string resolvedSourceName = casterPtr->NameString;
            uint resolvedSourceJobId = 0;

            bool sourceInFilter = IsEntityInFilter(casterEntityId);

            if (casterPtr->GameObject.OwnerId != InvalidGameObjectId)
            {
                var owner = Service.ObjectTable.SearchById(casterPtr->GameObject.OwnerId);
                var casterObj = Service.ObjectTable.SearchById(casterEntityId);

                if (System.Config.InternalParser.ShowCompanions &&
                    casterObj is IBattleNpc npc && IsCompanionNpc(npc))
                {
                    resolvedSourceJobId = npc.ClassJob.RowId;
                }
                else if (System.Config.InternalParser.MergePetDamage && owner is IPlayerCharacter pcOwner)
                {
                    resolvedSourceId = pcOwner.GameObjectId;
                    resolvedSourceName = pcOwner.Name.TextValue;
                    resolvedSourceJobId = pcOwner.ClassJob.RowId;
                }
            }
            else if (Service.ObjectTable.SearchById(casterEntityId) is IPlayerCharacter pc)
            {
                resolvedSourceJobId = pc.ClassJob.RowId;
            }

            var actionId = (ActionType)header->ActionType switch
            {
                ActionType.Mount => MountActionOffset + header->ActionId,
                ActionType.Item => ItemActionOffset + header->ActionId,
                _ => header->SpellId
            };

            var isLimitBreak = false;
            if (actionId is > 0 and < MountActionOffset and < ItemActionOffset)
            {
                var action = ActionSheet.GetRowOrDefault(actionId);
                if (action.HasValue)
                {
                    isLimitBreak = action.Value.ActionCategory.RowId == 9;
                }
            }

            for (var i = 0; i < header->NumTargets; i++)
            {
                var targetId = (uint)(targetEntityIds[i] & uint.MaxValue);
                if (IsDuplicateAction(casterEntityId, header->GlobalSequence, targetId, i)) continue;
                if (header->NumTargets == 1)
                    ObserveDirectHit(casterEntityId, targetId, actionId, ref effects[i]);

                for (var j = 0; j < 8; j++)
                {
                    ref var effect = ref effects[i].Effects[j];
                    var type = (ActionEffectType)effect.Type;
                    if (type is ActionEffectType.ApplyStatusTarget or ActionEffectType.ApplyStatusSource)
                    {
                        var recipientId = type == ActionEffectType.ApplyStatusSource ? casterEntityId : targetId;
                        statusTracker.ObserveApplication(recipientId, casterEntityId, effect.Value, actionId, header->GlobalSequence, effect.Param0);
                        if (Capture.IsActive)
                        {
                            var source = Service.ObjectTable.SearchById(casterEntityId) as IBattleChara;
                            Capture.Record("StatusApplication", new
                            {
                                SourceId = casterEntityId, TargetId = recipientId, ActionId = actionId,
                                SourceJobId = source?.ClassJob.RowId, SourceLevel = source?.Level,
                                StatusId = effect.Value, Sequence = header->GlobalSequence,
                                effect.Param0, effect.Param1, effect.Param2, effect.Param3, effect.Param4,
                            });
                        }
                    }
                }

                for (var j = 0; j < 8; j++)
                {
                    ref var effect = ref effects[i].Effects[j];
                    var type = (ActionEffectType)effect.Type;
                    if (type == ActionEffectType.Nothing) continue;

                    if (type != ActionEffectType.Damage &&
                        type != ActionEffectType.Heal &&
                        type != ActionEffectType.BlockedDamage &&
                        type != ActionEffectType.ParriedDamage &&
                        type != ActionEffectType.Miss) continue;

                    var amountFlags = (AmountFlags)effect.Param4;
                    var recipientId = amountFlags.HasFlag(AmountFlags.AppliesToSource) ? casterEntityId : targetId;
                    if (recipientId is 0 or InvalidGameObjectId || (!sourceInFilter && !IsEntityInFilter(recipientId))) continue;

                    var targetObj = Service.ObjectTable.SearchById(recipientId);
                    if (targetObj == null && type is not (ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)) continue;

                    uint amount = effect.Value;
                    if (amountFlags.HasFlag(AmountFlags.HasHighBytes))
                    {
                        amount += (uint)effect.Param3 << 16;
                    }

                    resolvedSourceName = GetResolvedName(resolvedSourceId, resolvedSourceName);

                    var targetName = GetResolvedName(recipientId, targetObj?.Name.TextValue ?? "");

                    var evt = new ActionResultEvent
                    {
                        TimestampUtc = receivedAt,
                        SourceId = resolvedSourceId,
                        SourceName = resolvedSourceName,
                        SourceJobId = resolvedSourceJobId,
                        TargetId = recipientId,
                        TargetName = targetName,
                        TargetCurrentHp = (targetObj as IBattleChara)?.CurrentHp ?? 0,
                        TargetMaxHp = (targetObj as IBattleChara)?.MaxHp ?? 0,
                        TargetJobId = targetObj switch
                        {
                            IPlayerCharacter tpc => tpc.ClassJob.RowId,
                            IBattleNpc tnpc when System.Config.InternalParser.ShowCompanions
                                                 && IsCompanionNpc(tnpc) => tnpc.ClassJob.RowId,
                            _ => 0
                        },
                        IsPlayerTarget = targetObj is IPlayerCharacter ||
                                         (targetObj != null && System.Config.InternalParser.ShowCompanions && IsCompanionNpc(targetObj)),
                        ActionId = actionId,
                    };

                    switch (type)
                    {
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                            var damageFlags = (EffectFlags)effect.Param0;
                            EmitActionResult(evt with {
                                Damage = amount,
                                IsCrit = damageFlags.HasFlag(EffectFlags.Critical),
                                IsDirectHit = damageFlags.HasFlag(EffectFlags.DirectHit),
                                IsLimitBreak = isLimitBreak,
                            });
                            break;

                        case ActionEffectType.Heal:
                            var healFlags = (EffectFlags)effect.Param1;
                            EmitActionResult(evt with {
                                Healing = amount,
                                IsCrit = healFlags.HasFlag(EffectFlags.Critical),
                                IsLimitBreak = isLimitBreak,
                            });
                            break;

                        case ActionEffectType.Miss:
                            EmitActionResult(evt with { Damage = 0, IsMiss = true, IsLimitBreak = isLimitBreak });
                            break;
                    }
                }
            }
        }
        catch (Exception ex) { Service.Logger.Error($"[Internal Parser] {ex}"); }
    }

    private bool IsDuplicateAction(uint sourceId, uint sequence, uint targetId, int index)
    {
        if (sequence == 0) return false;

        var identity = (sourceId, sequence, targetId, index);
        if (!seenActions.Add(identity)) return true;

        actionOrder.Enqueue(identity);
        if (actionOrder.Count > MaxTrackedActions) seenActions.Remove(actionOrder.Dequeue());
        return false;
    }

    private void ObserveDirectHit(uint sourceId, uint targetId, uint actionId, ref ActionEffectHandler.TargetEffects effects)
    {
        var damageEffects = 0;
        uint sample = 0;

        for (var i = 0; i < 8; i++)
        {
            ref var effect = ref effects.Effects[i];
            var type = (ActionEffectType)effect.Type;
            if (type is not (ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)) continue;

            damageEffects++;
            var flags = (EffectFlags)effect.Param0;
            var amountFlags = (AmountFlags)effect.Param4;
            if (type != ActionEffectType.Damage || (flags & (EffectFlags.Critical | EffectFlags.DirectHit)) != 0 ||
                amountFlags.HasFlag(AmountFlags.AppliesToSource)) continue;

            sample = effect.Value;
            if (amountFlags.HasFlag(AmountFlags.HasHighBytes)) sample += (uint)effect.Param3 << 16;
        }

        if (damageEffects != 1 || sample == 0) return;

        var scale = statusTracker.ObserveDirectHit(sourceId, targetId, actionId, sample);
        if (Capture.IsActive && scale != null)
            Capture.Record("Calibration", new { SourceId = sourceId, TargetId = targetId, ActionId = actionId, Amount = sample, Scale = scale });
    }

}
