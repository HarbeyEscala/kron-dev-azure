namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class TopMeasurementPointDto
{
    public int Rank { get; set; }

    public Guid MeasurementPointId { get; set; }

    public string MeasurementPointName { get; set; } = default!;

    public int Visitors { get; set; }
}