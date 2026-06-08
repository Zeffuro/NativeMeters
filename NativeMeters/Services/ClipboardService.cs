using System;
using System.Linq;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;

namespace NativeMeters.Services;

public static class ClipboardService
{
    public static unsafe void CopyEncounterSummary()
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

            SetClipboardText(sb.ToString());
            Service.NotificationManager.Success("Encounter copied to clipboard!");
        }
        catch (Exception)
        {
            Service.NotificationManager.Error("Failed to copy encounter summary");
        }
    }

    public static unsafe string GetClipboardText()
    {
        return AtkStage.Instance()->AtkInputManager->TextInput->ClipboardData.GetSystemClipboardText()->ToString();
    }

    public static unsafe void SetClipboardText(string text)
    {
        using var clipboardString = new Utf8String(text);
        using var clipboardOutput = new Utf8String();

        AtkStage.Instance()->AtkInputManager->TextInput->ClipboardData.WriteToSystemClipboard(&clipboardString, &clipboardOutput);
    }
}
