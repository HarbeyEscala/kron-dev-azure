namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class MeasurementPointDistributionDto
{
    public Guid MeasurementPointId { get; set; }

    public string MeasurementPointName { get; set; } = default!;

    public int Visitors { get; set; }

    public decimal TrafficPercentage { get; set; }
}