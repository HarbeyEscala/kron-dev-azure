namespace Kron.Counting.Application.DTOs.Internal;

public sealed class MonthlyMetricAggregationDto
{
    public Guid StoreId { get; set; }

    public short MetricYear { get; set; }

    public byte MetricMonth { get; set; }

    public int PeopleIn { get; set; }

    public int PeopleOut { get; set; }

    public int PeakOccupancy { get; set; }

    public decimal AvgOccupancy { get; set; }
}