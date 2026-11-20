using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Lumina.Excel.Exceptions;
using Lumina.Excel.Sheets;
using NativeMeters.Services;

namespace NativeMeters.Localization;

internal static class GameNames
{
    private static readonly ConcurrentDictionary<string, Dictionary<uint, string>> JobNames = new();

    internal static string JobName(ClassJob job)
    {
        if (job.RowId == 0) return string.Empty;
        return JobNames.GetOrAdd(Loc.DisplayLanguage, LoadJobNames).TryGetValue(job.RowId, out var name)
            ? name
            : Loc.GetOptional($"JobName_{job.RowId}") ?? job.NameEnglish.ToString();
    }

    private static Dictionary<uint, string> LoadJobNames(string displayLanguage)
    {
        if (!Enum.TryParse<ClientLanguage>(displayLanguage, out var language) || !Enum.IsDefined(language))
            return [];

        try
        {
            return Service.DataManager.GetExcelSheet<ClassJob>(language)
                .Select(job => (job.RowId, Name: language == ClientLanguage.English ? job.NameEnglish : job.Name))
                .Where(job => !job.Name.IsEmpty)
                .ToDictionary(job => job.RowId, job => job.Name.ToString());
        }
        catch (UnsupportedLanguageException)
        {
            return [];
        }
    }
}
