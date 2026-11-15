using NativeMeters.Models;

namespace NativeMeters.Tags;

public record CombatantTagContext(Combatant Combatant, int Rank, int CombatantCount)
{
    internal object RankValue { get; } = Rank;
    internal object CountValue { get; } = CombatantCount;
}
