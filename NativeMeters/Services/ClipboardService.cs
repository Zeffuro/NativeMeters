using System;
using System.Linq;
using System.Text;
using Dalamud.Bindings.ImGui;
using Lumina.Text.ReadOnly;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;

namespace NativeMeters.Services;

public static class ClipboardService
{
    public static void CopyEncounterSummary()
    {
        var encounter = System.ActiveMeterService.GetEncounter();
        var combatants = System.ActiveMeterService.GetCombatants().ToList();

        if (encounter == null || combatants.Count == 0)
            return;

        var sb = new StringBuilder();

        sb.AppendLine($"═══ {encounter.Title} ═══");
        sb.AppendLine($"Duration: {encounter.Duration:mm\\:ss}");
        sb.AppendLine();

        var selector = StatSelector.GetStatSelector("ENCDPS");
        combatants.Sort((a, b) => selector(b).CompareTo(selector(a)));

        int rank = 1;
        foreach (var c in combatants)
        {
            ReadOnlySeString name = System.Config.General.PrivacyMode ? c.Job.NameEnglish : c.Name;
            sb.AppendLine($"{rank}. {name} ({c.Job.Abbreviation}) - {c.ENCDPS:N0} DPS");
            rank++;
        }

        sb.AppendLine();
        sb.AppendLine($"Deaths: {encounter.Deaths} | Raid DPS: {encounter.ENCDPS:N0}");

        try
        {
            ImGui.SetClipboardText(sb.ToString());
            Service.NotificationManager.Success("Encounter copied to clipboard!");
        }
        catch (Exception ex)
        {
            Service.NotificationManager.Error("Failed to copy to clipboard.");
        }
    }
}
