using System.Numerics;
using Lumina.Excel.Sheets;
using NativeMeters.Models;

namespace NativeMeters.Extensions;

public static class ClassJobExtensions
{
    public static uint? GetIconId(this ClassJob classJob, JobIconType iconType = JobIconType.Default)
        => JobIconMaps.GetIcon(classJob.RowId, iconType);

    public static Vector4 GetColor(this ClassJob classJob)
        => JobColorMaps.DefaultColors.TryGetValue(classJob.RowId, out var color) ? color : JobColorMaps.DefaultColors[0];
}