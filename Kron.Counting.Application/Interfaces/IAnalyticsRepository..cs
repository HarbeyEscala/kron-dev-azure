using Kron.Counting.Application.DTOs.Analytics;

namespace Kron.Counting.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<IEnumerable<HourlyAnalyticsDto>> GetHourlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<DailyAnalyticsDto>> GetDailyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<MonthlyAnalyticsDto>> GetMonthlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<AnalyticsKpiDto> GetKpisAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<GrowthAnalyticsDto> GetGrowthAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<OccupancyAnalyticsDto> GetOccupancyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<ComparisonAnalyticsDto> GetComparisonAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<TrendAnalyticsDto>> GetTrendsAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<TopDayAnalyticsDto>> GetTopDaysAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10);

    Task<IEnumerable<TopStoreAnalyticsDto>> GetTopStoresAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10);

    Task<IEnumerable<OccupancyTrendDto>> GetOccupancyTrendsAsync(
        Guid storeId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<StoreComparisonAnalyticsDto>> GetStoreComparisonAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10);

    Task<IEnumerable<DayComparisonAnalyticsDto>> GetDayComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<HourComparisonAnalyticsDto>> GetHourComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<StoreVsStoreHourDto>> GetStoreVsStoreHourAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<StoreVsStoreDayDto>> GetStoreVsStoreDayAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc);
    Task<IEnumerable<MeasurementPointAnalyticsDto>> GetMeasurementPointAnalyticsAsync(
        Guid storeId,
        DateTime fromUtc,
        DateTime toUtc);

    Task<IEnumerable<TopMeasurementPointDto>>
        GetTopMeasurementPointsAsync(
            Guid storeId,
            DateTime fromUtc,
            DateTime toUtc,
            int top = 10);

    Task<IEnumerable<MeasurementPointDistributionDto>>
        GetMeasurementPointDistributionAsync(
            Guid storeId,
            DateTime fromUtc,
            DateTime toUtc);

    Task<MeasurementPointComparisonDto?>
        GetMeasurementPointComparisonAsync(
            Guid primaryMeasurementPointId,
            Guid comparisonMeasurementPointId,
            DateTime fromUtc,
            DateTime toUtc);
}