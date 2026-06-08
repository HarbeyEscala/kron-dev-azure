using Kron.Counting.Application.DTOs.Analytics;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly ICacheService _cacheService;
    public AnalyticsService(
        IAnalyticsRepository analyticsRepository,
        ICacheService cacheService)
    {
        _analyticsRepository = analyticsRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<HourlyAnalyticsDto>> GetHourlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetHourlyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<DailyAnalyticsDto>> GetDailyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetDailyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<MonthlyAnalyticsDto>> GetMonthlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetMonthlyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<AnalyticsKpiDto> GetKpisAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var cacheKey =
            $"analytics:kpis:{storeId}:{deviceId}:{fromUtc:yyyyMMdd}:{toUtc:yyyyMMdd}";

        var cached =
            await _cacheService.GetAsync<AnalyticsKpiDto>(
                cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var result =
            await _analyticsRepository.GetKpisAsync(
                storeId,
                deviceId,
                fromUtc,
                toUtc);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<GrowthAnalyticsDto> GetGrowthAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var cacheKey =
            $"analytics:growth:{storeId}:{deviceId}:{fromUtc:yyyyMMdd}:{toUtc:yyyyMMdd}";

        var cached =
            await _cacheService.GetAsync<GrowthAnalyticsDto>(
                cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var result =
            await _analyticsRepository.GetGrowthAsync(
                storeId,
                deviceId,
                fromUtc,
                toUtc);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<OccupancyAnalyticsDto> GetOccupancyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetOccupancyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<ComparisonAnalyticsDto> GetComparisonAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetComparisonAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<TrendAnalyticsDto>> GetTrendsAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetTrendsAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<TopDayAnalyticsDto>> GetTopDaysAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc,
    int top = 10)
    {
        var cacheKey =
            $"analytics:topdays:{storeId}:{deviceId}:{fromUtc:yyyyMMdd}:{toUtc:yyyyMMdd}:{top}";

        var cached =
            await _cacheService.GetAsync<
                IEnumerable<TopDayAnalyticsDto>>(
                    cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var result =
            await _analyticsRepository.GetTopDaysAsync(
                storeId,
                deviceId,
                fromUtc,
                toUtc,
                top);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }
    public async Task<IEnumerable<TopStoreAnalyticsDto>> GetTopStoresAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10)
    {
        var cacheKey =
            $"analytics:topstores:{fromUtc:yyyyMMdd}:{toUtc:yyyyMMdd}:{top}";

        var cached =
            await _cacheService.GetAsync<
                IEnumerable<TopStoreAnalyticsDto>>(
                    cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var result =
            await _analyticsRepository.GetTopStoresAsync(
                fromUtc,
                toUtc,
                top);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var cacheKey =
            $"analytics:summary:{storeId}:{fromUtc:yyyyMMdd}:{toUtc:yyyyMMdd}";

        var cached =
            await _cacheService.GetAsync<AnalyticsSummaryDto>(
                cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var kpis = await _analyticsRepository.GetKpisAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        var growth = await _analyticsRepository.GetGrowthAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        var occupancy = await _analyticsRepository.GetOccupancyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        var topDay = (await _analyticsRepository.GetTopDaysAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc,
            1))
            .FirstOrDefault();

        var topStore = (await _analyticsRepository.GetTopStoresAsync(
            fromUtc,
            toUtc,
            1))
            .FirstOrDefault();

        var summary = new AnalyticsSummaryDto
        {
            Kpis = kpis,
            Growth = growth,
            Occupancy = occupancy,
            TopDay = topDay,
            TopStore = topStore
        };

        await _cacheService.SetAsync(
            cacheKey,
            summary,
            TimeSpan.FromMinutes(5));

        return summary;
    }

    public async Task<IEnumerable<OccupancyTrendDto>> GetOccupancyTrendsAsync(
        Guid storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetOccupancyTrendsAsync(
            storeId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<StoreComparisonAnalyticsDto>> GetStoreComparisonAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10)
    {
        return await _analyticsRepository.GetStoreComparisonAsync(
            fromUtc,
            toUtc,
            top);
    }

    public async Task<IEnumerable<DayComparisonAnalyticsDto>> GetDayComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetDayComparisonAsync(
            storeId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<HourComparisonAnalyticsDto>> GetHourComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetHourComparisonAsync(
            storeId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<StoreVsStoreHourDto>> GetStoreVsStoreHourAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetStoreVsStoreHourAsync(
            primaryStoreId,
            comparisonStoreId,
            fromUtc,
            toUtc);
    }

    public async Task<IEnumerable<StoreVsStoreDayDto>> GetStoreVsStoreDayAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        return await _analyticsRepository.GetStoreVsStoreDayAsync(
            primaryStoreId,
            comparisonStoreId,
            fromUtc,
            toUtc);
    }
}