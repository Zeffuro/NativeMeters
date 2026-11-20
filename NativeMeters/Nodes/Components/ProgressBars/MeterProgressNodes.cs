using KamiToolKit.BaseTypes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components.ProgressBars;

internal static class MeterProgressNodes
{
    public static NodeBase Create(ProgressBarType type, bool preserveEnds = false) => type switch
    {
        ProgressBarType.Cast => new ProgressBarCastGaugeNode(preserveEnds),
        ProgressBarType.EnemyCast => new ProgressBarEnemyCastGaugeNode(preserveEnds),
        ProgressBarType.ToDo => new ProgressBarToDoGaugeNode(preserveEnds),
        ProgressBarType.PartyListHp => new ProgressBarPartyListHpNode(preserveEnds),
        ProgressBarType.LimitBreak => new ProgressBarLimitBreakGaugeNode(preserveEnds),
        ProgressBarType.ContentGauge => new ProgressBarContentGaugeNode(preserveEnds),
        ProgressBarType.BardArrow => new ProgressBarBardArrowNode(preserveEnds),
        ProgressBarType.AetherGauge or ProgressBarType.CustomDeliveries or ProgressBarType.ItemInspection or
            ProgressBarType.CollectableNegotiation or ProgressBarType.GoldSaucerGauge => new StockStripGaugeNode(type, preserveEnds),
        ProgressBarType.CastLegacy => new LegacyCastProgressBarNode(),
        ProgressBarType.ToDoLegacy => new LegacyToDoProgressBarNode(),
        ProgressBarType.EnemyCastLegacy => new LegacyEnemyCastProgressBarNode(),
        _ => new ProgressBarToDoGaugeNode(),
    };
}
