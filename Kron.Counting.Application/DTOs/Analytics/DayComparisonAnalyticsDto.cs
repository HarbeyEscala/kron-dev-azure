namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class DayComparisonAnalyticsDto
{
    public string DayName { get; set; } = string.Empty;

    public int CurrentVisitors { get; set; }

    public int PreviousVisitors { get; set; }
}