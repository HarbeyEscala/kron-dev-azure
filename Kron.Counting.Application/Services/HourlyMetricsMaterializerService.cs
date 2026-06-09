using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;

namespace Kron.Counting.Application.Services;

public sealed class HourlyMetricsMaterializerService
    : IHourlyMetricsMaterializerService
{
    private const int MaterializationWindowHours = 48;

    private readonly IHourlyMaterializationRepository _repository;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public HourlyMetricsMaterializerService(
        IHourlyMaterializationRepository repository,
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
        var fromUtc =
            DateTime.UtcNow.AddHours(-MaterializationWindowHours);

        var metrics =
            await _repository.GetHourlyAggregationsAsync(
                fromUtc,
                cancellationToken);

        var metricList = metrics.ToList();

        if (metricList.Count == 0)
            return;

        await _repository.UpsertHourlyMetricsAsync(
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