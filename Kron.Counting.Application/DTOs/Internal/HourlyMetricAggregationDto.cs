namespace Kron.Counting.Application.DTOs.Internal;

public sealed class HourlyMetricAggregationDto
{
    public Guid StoreId { get; set; }

    public DateOnly MetricDate { get; set; }

    public byte MetricHour { get; set; }

    public int PeopleIn { get; set; }

    public int PeopleOut { get; set; }

    public int PeakOccupancy { get; set; }

    public decimal AvgOccupancy { get; set; }
}