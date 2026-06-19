using Kron.Counting.Application.Interfaces;
using Kron.Counting.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/v1/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(
        IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("hourly")]
    public async Task<IActionResult> GetHourly(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetHourlyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetDailyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetMonthlyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetKpisAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("growth")]
    public async Task<IActionResult> GetGrowth(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetGrowthAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetOccupancyAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("comparison")]
    public async Task<IActionResult> GetComparison(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetComparisonAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends(
    [FromQuery] Guid? storeId,
    [FromQuery] Guid? deviceId,
    [FromQuery] DateTime fromUtc,
    [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetTrendsAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("top-days")]
    public async Task<IActionResult> GetTopDays(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int top = 10)
    {
        var result = await _analyticsService.GetTopDaysAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc,
            top);

        return Ok(result);
    }

    [HttpGet("top-stores")]
    public async Task<IActionResult> GetTopStores(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int top = 10)
    {
        var result = await _analyticsService.GetTopStoresAsync(
            fromUtc,
            toUtc,
            top);

        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? deviceId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetSummaryAsync(
            storeId,
            deviceId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("occupancy-trends")]
    public async Task<IActionResult> GetOccupancyTrends(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetOccupancyTrendsAsync(
            storeId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("comparison/store")]
    public async Task<IActionResult> GetStoreComparison(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int top = 10)
    {
        var result = await _analyticsService.GetStoreComparisonAsync(
            fromUtc,
            toUtc,
            top);

        return Ok(result);
    }

    [HttpGet("comparison/day")]
    public async Task<IActionResult> GetDayComparison(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetDayComparisonAsync(
            storeId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("comparison/hour")]
    public async Task<IActionResult> GetHourComparison(
        [FromQuery] Guid? storeId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetHourComparisonAsync(
            storeId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("store-vs-store/hour")]
    public async Task<IActionResult> GetStoreVsStoreHour(
        [FromQuery] Guid primaryStoreId,
        [FromQuery] Guid comparisonStoreId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetStoreVsStoreHourAsync(
            primaryStoreId,
            comparisonStoreId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("store-vs-store/day")]
    public async Task<IActionResult> GetStoreVsStoreDay(
        [FromQuery] Guid primaryStoreId,
        [FromQuery] Guid comparisonStoreId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result = await _analyticsService.GetStoreVsStoreDayAsync(
            primaryStoreId,
            comparisonStoreId,
            fromUtc,
            toUtc);

        return Ok(result);
    }

    [HttpGet("measurement-points")]
    public async Task<IActionResult> GetMeasurementPointAnalytics(
    [FromQuery] Guid storeId,
    [FromQuery] DateTime fromUtc,
    [FromQuery] DateTime toUtc)
    {
        var result =
            await _analyticsService
                .GetMeasurementPointAnalyticsAsync(
                    storeId,
                    fromUtc,
                    toUtc);

        return Ok(result);
    }

    [HttpGet("measurement-points/top")]
    public async Task<IActionResult> GetTopMeasurementPoints(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int top = 10)
    {
        var result =
            await _analyticsService
                .GetTopMeasurementPointsAsync(
                    storeId,
                    fromUtc,
                    toUtc,
                    top);

        return Ok(result);
    }

    [HttpGet("measurement-points/distribution")]
    public async Task<IActionResult> GetMeasurementPointDistribution(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result =
            await _analyticsService
                .GetMeasurementPointDistributionAsync(
                    storeId,
                    fromUtc,
                    toUtc);

        return Ok(result);
    }

    [HttpGet("measurement-points/comparison")]
    public async Task<IActionResult> GetMeasurementPointComparison(
        [FromQuery] Guid primaryMeasurementPointId,
        [FromQuery] Guid comparisonMeasurementPointId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var result =
            await _analyticsService
                .GetMeasurementPointComparisonAsync(
                    primaryMeasurementPointId,
                    comparisonMeasurementPointId,
                    fromUtc,
                    toUtc);

        return Ok(result);
    }

    [HttpGet("stores-map")]
    public async Task<IActionResult> GetStoresMap(
        [FromQuery] Guid tenantId)
    {
        var result =
            await _analyticsService
                .GetStoresMapAsync(
                    tenantId);

        return Ok(result);
    }

    // FORECAST - PREDICCION POR ESTADISTICA-V1

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime? targetDateUtc)
    {
        var result =
            await _analyticsService.GetForecastAsync(
                storeId,
                targetDateUtc ?? DateTime.UtcNow.Date);

        return Ok(result);
    }
}