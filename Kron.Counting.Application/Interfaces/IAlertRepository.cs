using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IAlertRepository
{
    Task<Guid> CreateAsync(
        Alert alert);

    Task<Alert?> GetOpenAlertAsync(
        Guid tenantId,
        Guid? deviceId,
        string type);

    Task TouchAsync(
        Guid alertId);

    Task ResolveAsync(
        Guid alertId);

    Task<IReadOnlyList<Alert>> GetOpenAlertsAsync(
        Guid tenantId);

    Task<IReadOnlyList<Alert>> GetHistoryAsync(
        Guid tenantId,
        int take = 100);

    Task<IReadOnlyList<Alert>> GetByDeviceAsync(
        Guid tenantId,
        Guid deviceId);

    Task<IReadOnlyList<Alert>> GetByStoreAsync(
        Guid tenantId,
        Guid storeId);
}