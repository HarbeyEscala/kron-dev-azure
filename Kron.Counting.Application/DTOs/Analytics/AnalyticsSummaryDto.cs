namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class AnalyticsSummaryDto
{
    public AnalyticsKpiDto Kpis { get; set; } = new();

    public GrowthAnalyticsDto Growth { get; set; } = new();

    public OccupancyAnalyticsDto Occupancy { get; set; } = new();

    public TopDayAnalyticsDto? TopDay { get; set; }

    public TopStoreAnalyticsDto? TopStore { get; set; }

}