using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Device?> GetBySerialNumberAsync(Guid storeId, string serialNumber, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Device device, CancellationToken cancellationToken = default);

    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);

    Task UpdateHeartbeatAsync(
        Guid id,
        DateTime lastSeenAtUtc,
        bool isOnline,
        CancellationToken cancellationToken = default);

    Task<Device?> GetByApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default);

    Task<Device?> GetByIpAddressAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<Device?> GetBySerialNumberAsync(
        string serialNumber,
        CancellationToken cancellationToken);

    Task<IEnumerable<Device>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task ProvisionAsync(
        Guid deviceId,
        Guid tenantId,
        Guid storeId,
        string name,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);


}