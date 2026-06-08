namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class HourlyAnalyticsDto
{
    public DateTime HourUtc { get; set; }
    public int PeopleIn { get; set; }
    public int PeopleOut { get; set; }
}