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
        try
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
            combatants.Sort((left, right) => selector(right).CompareTo(selector(left)));

            int rank = 1;
            foreach (var c in combatants)
            {
                bool hasValidJob = c.Job.RowId > 0;
                string jobAbbrev = hasValidJob ? c.Job.Abbreviation.ToString() : "LB";
                string name = System.Config.General.PrivacyMode && hasValidJob ? c.Job.NameEnglish.ToString() : c.Name;

                sb.AppendLine($"{rank}. {name} ({jobAbbrev}) - {c.ENCDPS:N0} DPS");
                rank++;
            }

            sb.AppendLine();
            sb.AppendLine($"Deaths: {encounter.Deaths} | Raid DPS: {encounter.ENCDPS:N0}");

            var text = sb.ToString();

            Service.Framework.RunOnFrameworkThread(() =>
            {
                try
                {
                    ImGui.SetClipboardText(text);
                    Service.NotificationManager.Success("Encounter copied to clipboard!");
                }
                catch (Exception ex)
                {
                    Service.NotificationManager.Error("Failed to copy to clipboard.");
                }
            });
        }
        catch (Exception ex)
        {
            Service.NotificationManager.Error("Failed to copy encounter summary");
        }
    }
}
