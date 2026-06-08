using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class DailyMetricsMaterializerService
    : IDailyMetricsMaterializerService
{
    private readonly IMaterializationStateRepository _stateRepository;
    private readonly IDailyMaterializationRepository _repository;

    public DailyMetricsMaterializerService(
        IMaterializationStateRepository stateRepository,
        IDailyMaterializationRepository repository)
    {
        _stateRepository = stateRepository;
        _repository = repository;
    }

    public async Task MaterializeAsync(
        CancellationToken cancellationToken = default)
    {
        var state =
            await _stateRepository.GetByProcessAsync(
                "DailyMetrics");

        if (state is null)
        {
            state = new MaterializationState
            {
                ProcessName = "DailyMetrics",
                LastProcessedUtc = DateTime.UtcNow.AddDays(-30),
                UpdatedAtUtc = DateTime.UtcNow
            };

            state.Id =
                await _stateRepository.CreateAsync(state);
        }

        var fromDate =
            DateOnly.FromDateTime(state.LastProcessedUtc.Date);

        var metrics =
            await _repository.GetDailyAggregationsAsync(
                fromDate,
                cancellationToken);

        var metricList = metrics.ToList();

        if (metricList.Count == 0)
            return;

        await _repository.UpsertDailyMetricsAsync(
            metricList,
            cancellationToken);

        state.LastProcessedUtc = DateTime.UtcNow;
        state.UpdatedAtUtc = DateTime.UtcNow;

        await _stateRepository.UpdateAsync(state);
    }
}