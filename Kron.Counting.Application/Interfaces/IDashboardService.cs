using Kron.Counting.Application.DTOs.Responses;

namespace Kron.Counting.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSnapshotDto> GetSnapshotAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoreHourlyMetricDto>> GetHourlyMetricsAsync(
        Guid storeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoreDailyMetricDto>> GetDailyMetricsAsync(
        Guid storeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}