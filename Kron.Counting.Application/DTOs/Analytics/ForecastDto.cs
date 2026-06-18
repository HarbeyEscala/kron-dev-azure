namespace Kron.Counting.Application.DTOs.Analytics;

public sealed class ForecastDto
{
    public Guid StoreId { get; set; }

    public DateTime TargetDateUtc { get; set; }

    public int PredictedVisitors { get; set; }

    public int PredictedPeopleOut { get; set; }

    public int PredictedNetTraffic { get; set; }

    public int PredictedPeakHour { get; set; }

    public int PredictedPeakHourVisitors { get; set; }

    public int PredictedPeakOccupancy { get; set; }

    public decimal Confidence { get; set; }

    public int HistoricalDaysUsed { get; set; }

    public string Method { get; set; } = "EquivalentWeekdayAverageV1";
}