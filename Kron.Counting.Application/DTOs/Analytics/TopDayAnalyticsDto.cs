namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class TopDayAnalyticsDto
{
    public DateTime DateUtc { get; set; }

    public int Visitors { get; set; }
}