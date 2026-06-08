namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class HourComparisonAnalyticsDto
{
    public int Hour { get; set; }

    public int CurrentVisitors { get; set; }

    public int PreviousVisitors { get; set; }
}