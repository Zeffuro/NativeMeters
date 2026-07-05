using System.ComponentModel;

namespace NativeMeters.Models.Breakdown;

public enum BreakdownSortMode
{
    [Description("DPS / HPS")]
    Output,

    Name,

    Job,

    Role,
}
