using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupSampleFight
{
    private const int DurationSeconds = 98;
    public List<Combatant> Members { get; } =
    [
        Create("Ari Vale", "PLD", 13_840, 320, "Confiteor-68800", 18, 17, 102, 0),
        Create("Tala Reed", "WHM", 10_460, 6_410, "Afflatus Misery-82640", 22, 18, 75, 1),
        Create("Ren Ash", "BLM", 21_730, 0, "Xenoglossy-97000", 24, 26, 94, 0),
    ];
    public Encounter Encounter { get; }

    public SetupSampleFight(string title = "Striking Dummy")
    {
        Encounter = FakeEncounterFactory.CreateFakeEncounter(Members);
        Encounter.Title = title;
        Encounter.CurrentZoneName = "The Goblet";
        foreach (var member in Members)
            member.DamagePercent = member.Damage * 100.0 / Encounter.Damage;
    }

    private static Combatant Create(string name, string job, int dps, int hps, string bestHit,
        int crit, int direct, int hits, int deaths)
        => new()
        {
            Name = name,
            Job = Service.DataManager.GetClassJobByAbbreviation(job),
            Duration = TimeSpan.FromSeconds(DurationSeconds),
            DURATION = DurationSeconds,
            ENCDPS = dps,
            Encdps = dps,
            DPS = dps,
            Dps = dps,
            ENCHPS = hps,
            Enchps = hps,
            Damage = dps * DurationSeconds,
            DamageM = dps * DurationSeconds / 1_000_000.0,
            Maxhit = bestHit,
            CrithitPercent = crit,
            DirectHitPct = direct,
            CritDirectHitPct = crit * direct / 100.0,
            Swings = hits,
            Hits = hits,
            Deaths = deaths,
        };
}

internal static class SetupHudPreview
{
    public static TextNode ServerInfo(SetupSampleFight sample, string format, bool personalDps, float availableWidth)
        => new()
        {
            Position = new Vector2(0, 2),
            Size = new Vector2(availableWidth, 22),
            FontType = FontType.Axis,
            FontSize = 14,
            LineSpacing = 12,
            AlignmentType = AlignmentType.Left,
            TextFlags = TextFlags.Edge | TextFlags.Ellipsis,
            TextColor = Vector4.One,
            TextOutlineColor = new Vector4(142, 106, 12, 255) / 255,
            String = FormatServerInfo(sample, format, personalDps),
        };

    public static string FormatServerInfo(SetupSampleFight sample, string format, bool personalDps)
        => DtrService.FormatStats(format, sample.Encounter, sample.Members[0], personalDps);
}
