using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class MonthlyMetricsMaterializerService
    : IMonthlyMetricsMaterializerService
{
    private readonly IMaterializationStateRepository _stateRepository;
    private readonly IMonthlyMaterializationRepository _repository;

    public MonthlyMetricsMaterializerService(
        IMaterializationStateRepository stateRepository,
        IMonthlyMaterializationRepository repository)
    {
        _stateRepository = stateRepository;
        _repository = repository;
    }

    public async Task MaterializeAsync(
        CancellationToken cancellationToken = default)
    {
        var state =
            await _stateRepository.GetByProcessAsync(
                "MonthlyMetrics");

        if (state is null)
        {
            state = new MaterializationState
            {
                ProcessName = "MonthlyMetrics",
                LastProcessedUtc = DateTime.UtcNow.AddYears(-1),
                UpdatedAtUtc = DateTime.UtcNow
            };

            state.Id =
                await _stateRepository.CreateAsync(state);
        }

        var metrics =
            await _repository.GetMonthlyAggregationsAsync(
                DateOnly.FromDateTime(
                    state.LastProcessedUtc.Date),
                cancellationToken);

        var list = metrics.ToList();

        if (!list.Any())
            return;

        await _repository.UpsertMonthlyMetricsAsync(
            list,
            cancellationToken);

        state.LastProcessedUtc = DateTime.UtcNow;
        state.UpdatedAtUtc = DateTime.UtcNow;

        await _stateRepository.UpdateAsync(state);
    }
}