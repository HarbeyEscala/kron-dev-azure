using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class HourlyMetricsMaterializerService
    : IHourlyMetricsMaterializerService
{
    private readonly IMaterializationStateRepository _stateRepository;
    private readonly IHourlyMaterializationRepository _repository;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public HourlyMetricsMaterializerService(
        IMaterializationStateRepository stateRepository,
        IHourlyMaterializationRepository repository,
        ICacheInvalidationService cacheInvalidationService,
        IRealtimeNotificationService realtimeNotificationService)
    {
        _stateRepository = stateRepository;
        _repository = repository;
        _cacheInvalidationService = cacheInvalidationService;
        _realtimeNotificationService = realtimeNotificationService;
    }

    public async Task MaterializeAsync(
        CancellationToken cancellationToken = default)
    {
        var state =
            await _stateRepository.GetByProcessAsync(
                "HourlyMetrics");

        if (state is null)
        {
            state = new MaterializationState
            {
                ProcessName = "HourlyMetrics",
                LastProcessedUtc = DateTime.UtcNow.AddDays(-30),
                UpdatedAtUtc = DateTime.UtcNow
            };

            state.Id =
                await _stateRepository.CreateAsync(state);
        }

        var metrics =
            await _repository.GetHourlyAggregationsAsync(
                state.LastProcessedUtc,
                cancellationToken);

        var metricList = metrics.ToList();

        if (metricList.Count == 0)
            return;

        await _repository.UpsertHourlyMetricsAsync(
            metricList,
            cancellationToken);

        state.LastProcessedUtc = DateTime.UtcNow;
        state.UpdatedAtUtc = DateTime.UtcNow;

        await _stateRepository.UpdateAsync(state);

        await _cacheInvalidationService
            .InvalidateAnalyticsAsync(
                cancellationToken);

        await _realtimeNotificationService
            .AnalyticsUpdatedAsync(
                cancellationToken);
    }
}