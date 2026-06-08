namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class DailyAnalyticsDto
{
    public DateTime DateUtc { get; set; }
    public int PeopleIn { get; set; }
    public int PeopleOut { get; set; }
}