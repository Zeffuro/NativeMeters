using System;
using System.Linq;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Localization;

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
            sb.AppendLine(Loc.Format("ClipboardDuration", encounter.Duration.ToString(@"mm\:ss")));
            sb.AppendLine();

            var selector = StatSelector.GetStatSelector("ENCDPS");
            combatants.Sort((left, right) => selector(right).CompareTo(selector(left)));

            int rank = 1;
            foreach (var c in combatants)
            {
                bool hasValidJob = c.Job.RowId > 0;
                string jobAbbrev = hasValidJob ? c.Job.Abbreviation.ToString() : "LB";
                string name = System.Config.General.PrivacyMode && hasValidJob ? GameNames.JobName(c.Job) : c.Name;

                if (c.IsLimitBreak) name = Loc.Get("HudLimitBreak");
                sb.AppendLine(Loc.Format("ClipboardPlayer", rank, name, jobAbbrev, c.ENCDPS));
                rank++;
            }

            sb.AppendLine();
            sb.AppendLine(Loc.Format("ClipboardTotals", encounter.Deaths, encounter.ENCDPS));

            SetClipboardText(sb.ToString());
            Service.NotificationManager.Success(Loc.Get("ClipboardCopied"));
        }
        catch (Exception)
        {
            Service.NotificationManager.Error(Loc.Get("ClipboardCopyFailed"));
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
