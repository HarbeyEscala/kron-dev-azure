namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class TrendAnalyticsDto
{
    public DateTime DateUtc { get; set; }

    public int Visitors { get; set; }

    public int Exits { get; set; }
}