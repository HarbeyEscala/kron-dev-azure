using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class HourlyMetricsMaterializerService
    : IHourlyMetricsMaterializerService
{
    private const string ProcessName = "HourlyMetrics";

    private const int BootstrapHours = 48;

    private const int OverlapHours = 3;

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
                ProcessName);

        if (state is null)
        {
            state = new MaterializationState
            {
                ProcessName = ProcessName,
                LastProcessedUtc =
                    DateTime.UtcNow.AddHours(-BootstrapHours),
                UpdatedAtUtc =
                    DateTime.UtcNow
            };

            state.Id =
                await _stateRepository.CreateAsync(state);
        }

        var fromUtc =
            state.LastProcessedUtc
                .AddHours(-OverlapHours);

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

        state.LastProcessedUtc =
            DateTime.UtcNow;

        state.UpdatedAtUtc =
            DateTime.UtcNow;

        await _stateRepository.UpdateAsync(state);

        await _cacheInvalidationService
            .InvalidateAnalyticsAsync(
                cancellationToken);

        await _realtimeNotificationService
            .AnalyticsUpdatedAsync(
                cancellationToken);
    }
}
