namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class OccupancyTrendDto
{
    public DateTime HourUtc { get; set; }
    public int Occupancy { get; set; }
}