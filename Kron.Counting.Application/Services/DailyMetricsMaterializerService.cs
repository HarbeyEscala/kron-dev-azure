using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;

namespace Kron.Counting.Application.Services;

public sealed class DailyMetricsMaterializerService
    : IDailyMetricsMaterializerService
{
    private readonly IDailyMaterializationRepository _repository;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public DailyMetricsMaterializerService(
        IDailyMaterializationRepository repository,
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
                DateTime.UtcNow.AddDays(-30));

        var metrics =
            await _repository.GetDailyAggregationsAsync(
                fromDate,
                cancellationToken);

        var metricList =
            metrics.ToList();

        if (metricList.Count == 0)
            return;

        await _repository.UpsertDailyMetricsAsync(
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