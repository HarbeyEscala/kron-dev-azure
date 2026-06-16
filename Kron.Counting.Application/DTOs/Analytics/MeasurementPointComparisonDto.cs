namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class MeasurementPointComparisonDto
{
    public Guid PrimaryMeasurementPointId { get; set; }

    public string PrimaryMeasurementPointName { get; set; } = default!;

    public Guid ComparisonMeasurementPointId { get; set; }

    public string ComparisonMeasurementPointName { get; set; } = default!;

    public int PrimaryVisitors { get; set; }

    public int ComparisonVisitors { get; set; }

    public int PrimaryPeakOccupancy { get; set; }

    public int ComparisonPeakOccupancy { get; set; }

    public int VisitorDifference { get; set; }

    public int OccupancyDifference { get; set; }
}