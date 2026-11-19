using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static NativeMeters.Configuration.Presets.MeterPresetComponents;
using static NativeMeters.Configuration.Presets.CustomMeterColumn;

namespace NativeMeters.Configuration.Presets;

internal static class ShowcaseMeterPresets
{
    private static readonly Dictionary<string, Action<MeterSettings>> Presets = new(StringComparer.Ordinal)
    {
        ["Compact Underline"] = ApplyCompactUnderline,
        ["Combat Table"] = target => CustomMeterPresets.ApplyVertical(target, CustomMeterBarStyle.None,
            [PlayerName, Dps, DamagePercent, Damage, Swings, Hits, DirectHitPercent, CriticalHitPercent,
                CriticalDirectHitPercent, BestHit, Deaths], showTotals: true),
        ["Party Cards"] = ApplyPartyCards,
        ["Compact Tiles"] = ApplyCompactTiles,
        ["Best Hit Strip"] = ApplyBestHitStrip,
    };

    internal static IEnumerable<string> Names => Presets.Keys;

    internal static bool TryApply(string name, MeterSettings target)
    {
        if (!Presets.TryGetValue(name, out var apply)) return false;
        apply(target);
        return true;
    }

    private static void ApplyCompactUnderline(MeterSettings target)
    {
        const float bottomPadding = 8f;
        CustomMeterPresets.ApplyVertical(target, CustomMeterBarStyle.Underline,
            [PlayerName, DamagePercent, Dps, CriticalHitPercent, DirectHitPercent], showTotals: false);
        var name = target.RowComponents.Find(component => component.Name == "Player Name")!;
        var bar = target.RowComponents.Find(component => component.Type == MeterComponentType.ProgressBar)!;
        var trackRight = bar.Position.X + bar.Size.X;
        var barLeft = MathF.Round((name.Position.X - trackRight * ToDoGaugeFillInset) / (1f - ToDoGaugeFillInset));
        bar.Position = bar.Position with { X = barLeft };
        bar.Size = bar.Size with { X = trackRight - barLeft };
        target.FooterEnabled = true;
        target.FooterHeight = bottomPadding;
        target.Size = target.Size with { Y = target.Size.Y + bottomPadding };
    }

    private static void ApplyPartyCards(MeterSettings target)
    {
        const float width = 152f;
        const float height = 87f;
        const float leftText = 8f;
        SetHorizontal(target, width, height, 6,
        [
            Background(width, height, CardTint),
            Text("Rank", "[rank]", leftText, 3, 21, 20),
            Text("Player Name", "[name]", 31, 3, 113, 20, 14),
            Job(63, 25, 26),
            Text("Healing", "[ENCHPS:c.1]", leftText, 24, 52, 16),
            Text("Healing Label", "HPS", leftText, 40, 52, 13, 11, color: Muted),
            Text("Damage", "[ENCDPS:c.1]", 92, 24, 52, 16, 14, AlignmentType.Right),
            Text("Damage Label", "DPS", 92, 40, 52, 13, 11, AlignmentType.Right, Muted),
            Text("Damage Share", "[damagepct.1]% of damage", 8, 55, 136, 16, 12, AlignmentType.Center),
            Bar(12, 71, 128, 12, ProgressBarType.PartyListHp),
        ], durationColor: Ink);
    }

    private static void ApplyCompactTiles(MeterSettings target)
    {
        const float width = 152f;
        const float height = 34f;
        SetHorizontal(target, width, height, 5,
        [
            Background(width, height, CardTint with { W = 0.80f }),
            Job(5, 4, 21),
            Text("Player Name", "[name]", 30, 3, 55, 21, 13),
            Text("DPS", "[ENCDPS:c.1]", 88, 3, 58, 21, 13, AlignmentType.Right),
            Bar(6, 27, 140, 4),
        ]);
    }

    private static void ApplyBestHitStrip(MeterSettings target)
    {
        const float width = 152f;
        const float height = 89f;
        var background = Vector4.Lerp(CardTint, Vector4.One, 0.40f) with { W = 0.78f };
        var bar = Bar(7, 36, 138, 20, ProgressBarType.Cast);
        bar.ProgressBarColorTreatment = ProgressBarColorTreatment.NativeTint;
        bar.BarBackgroundColor = bar.BarBackgroundColor with { W = 0.38f };
        SetHorizontal(target, width, height, 6,
        [
            Background(width, height, background),
            Text("Player Name", "[name]", 7, 1, 138, 17, 14, AlignmentType.Center, Ink with { W = 1f }),
            Job(23, 18, 18),
            Text("DPS", "[ENCDPS.0] DPS", 47, 18, 91, 18, 14, AlignmentType.Center),
            bar,
            Text("Best Hit Skill", "[maxhit_skill]", 7, 56, 138, 12, 11, AlignmentType.Center, Ink with { W = 0.90f }),
            Text("Best Hit Amount", "[maxhit_val.0]", 7, 68, 138, 13, 12, AlignmentType.Center, Accent),
        ]);
    }

    private static void SetHorizontal(MeterSettings target, float width, float height, float gap,
        List<ComponentSettings> rows, Vector4? durationColor = null)
        => SetLayout(target, MeterLayout.Horizontal, width, height, gap, 28,
            Header(width * DefaultCombatants + gap * (DefaultCombatants - 1), totals: false, durationColor), rows);
}
