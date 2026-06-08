using Kron.Counting.Application.DTOs.Internal;

namespace Kron.Counting.Application.Interfaces;

public interface IHourlyMaterializationRepository
{
    Task<IEnumerable<HourlyMetricAggregationDto>> GetHourlyAggregationsAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default);

    Task UpsertHourlyMetricsAsync(
        IEnumerable<HourlyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default);
}