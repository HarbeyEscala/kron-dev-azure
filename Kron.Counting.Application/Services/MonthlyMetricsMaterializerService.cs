using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;

namespace Kron.Counting.Application.Services;

public sealed class MonthlyMetricsMaterializerService
    : IMonthlyMetricsMaterializerService
{
    private readonly IMonthlyMaterializationRepository _repository;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public MonthlyMetricsMaterializerService(
        IMonthlyMaterializationRepository repository,
        ICacheInvalidationService cacheInvalidationService,
        IRealtimeNotificationService realtimeNotificationService)
    {
        _repository = repository;
        _cacheInvalidationService = cacheInvalidationService;
        _realtimeNotificationService = realtimeNotificationService;
    }

    public async Task MaterializeAsync(
    CancellationToken cancellationToken = default)
    {
        var fromDate =
            DateOnly.FromDateTime(
                DateTime.UtcNow.AddMonths(-12));

        var metrics =
            await _repository.GetMonthlyAggregationsAsync(
                fromDate,
                cancellationToken);

        var metricList =
            metrics.ToList();

        if (metricList.Count == 0)
            return;

        await _repository.UpsertMonthlyMetricsAsync(
            metricList,
            cancellationToken);

        await _cacheInvalidationService
            .InvalidateAnalyticsAsync(
                cancellationToken);

        await _realtimeNotificationService
            .AnalyticsUpdatedAsync(
                cancellationToken);
    }
}