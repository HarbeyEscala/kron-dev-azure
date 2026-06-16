namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class MeasurementPointAnalyticsDto
{
    public Guid MeasurementPointId { get; set; }

    public string MeasurementPointName { get; set; } = default!;

    public int Visitors { get; set; }

    public int Exits { get; set; }

    public int NetTraffic { get; set; }

    public int PeakOccupancy { get; set; }

    public decimal AvgOccupancy { get; set; }
}