namespace NativeMeters.Models.Breakdown;

public record BreakdownColumn(
    string Key,
    string Label,
    float Width,
    bool IsVisible = true,
    ColumnAlignment Alignment = ColumnAlignment.Right
);

public enum ColumnAlignment { Left, Right, Center }
