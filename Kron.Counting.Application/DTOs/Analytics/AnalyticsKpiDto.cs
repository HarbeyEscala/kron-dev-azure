namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class AnalyticsKpiDto
{
    public int TotalVisitors { get; set; }

    public int TotalExits { get; set; }

    public int NetTraffic { get; set; }

    public DateTime? PeakHourUtc { get; set; }

    public int PeakHourVisitors { get; set; }

    public DateTime? PeakDayUtc { get; set; }

    public int PeakDayVisitors { get; set; }

    public decimal AverageVisitorsPerDay { get; set; }

    public decimal AverageVisitorsPerHour { get; set; }

}