using Kron.Counting.Application.DTOs.Internal;

namespace Kron.Counting.Application.Interfaces;

public interface IDailyMaterializationRepository
{
    Task<IEnumerable<DailyMetricAggregationDto>> GetDailyAggregationsAsync(
        DateOnly fromDate,
        CancellationToken cancellationToken = default);

    Task UpsertDailyMetricsAsync(
        IEnumerable<DailyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default);
}