namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class MonthlyAnalyticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int PeopleIn { get; set; }
    public int PeopleOut { get; set; }
}