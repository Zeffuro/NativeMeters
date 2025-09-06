using System;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace NativeMeters.Extensions;

public static class DataManagerExtensions
{
    public static ClassJob GetClassJobByAbbreviation(this IDataManager dataManager, string abbreviation)
        => dataManager.GetExcelSheet<ClassJob>().FirstOrDefault(classJob => string.Equals(classJob.Abbreviation.ToString(), abbreviation, StringComparison.CurrentCultureIgnoreCase));
}