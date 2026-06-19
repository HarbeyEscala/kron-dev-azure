using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;

namespace Kron.Counting.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IStoreRepository _storeRepository;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        IStoreRepository storeRepository)
    {
        _dashboardRepository = dashboardRepository;
        _storeRepository = storeRepository;
    }

    public async Task<DashboardSnapshotDto> GetSnapshotAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var store =
            await _storeRepository.GetByIdAsync(
                storeId,
                cancellationToken);

        if (store is null)
            throw new KeyNotFoundException("Store not found.");

        var snapshot =
            await _dashboardRepository.GetSnapshotByStoreIdAsync(
                storeId,
                cancellationToken);

        if (snapshot is null)
        {
            return new DashboardSnapshotDto
            {
                StoreId = storeId,
                CurrentOccupancy = 0,
                TodayIn = 0,
                TodayOut = 0,
                LastReadingAtUtc = null,
                UpdatedAtUtc = default
            };
        }

        return snapshot.ToDto();
    }

    public async Task<IEnumerable<StoreHourlyMetricDto>> GetHourlyMetricsAsync(
        Guid storeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var store =
            await _storeRepository.GetByIdAsync(
                storeId,
                cancellationToken);

        if (store is null)
            throw new KeyNotFoundException("Store not found.");

        var metrics =
            await _dashboardRepository.GetHourlyMetricsAsync(
                storeId,
                fromDate,
                toDate,
                cancellationToken);

        return metrics.Select(x => x.ToDto());
    }

    public async Task<IEnumerable<StoreDailyMetricDto>> GetDailyMetricsAsync(
        Guid storeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var store =
            await _storeRepository.GetByIdAsync(
                storeId,
                cancellationToken);

        if (store is null)
            throw new KeyNotFoundException("Store not found.");

        var metrics =
            await _dashboardRepository.GetDailyMetricsAsync(
                storeId,
                fromDate,
                toDate,
                cancellationToken);

        return metrics.Select(x => x.ToDto());
    }
}