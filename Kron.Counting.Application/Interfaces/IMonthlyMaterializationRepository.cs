using Kron.Counting.Application.DTOs.Internal;

namespace Kron.Counting.Application.Interfaces;

public interface IMonthlyMaterializationRepository
{
    Task<IEnumerable<MonthlyMetricAggregationDto>>
        GetMonthlyAggregationsAsync(
            DateOnly fromDate,
            CancellationToken cancellationToken = default);

    Task UpsertMonthlyMetricsAsync(
        IEnumerable<MonthlyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default);
}