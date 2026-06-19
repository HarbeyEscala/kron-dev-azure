using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);

    Task<DeviceDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DeviceApiKeyDto> GetApiKeyAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);

    Task<DeviceApiKeyDto> RotateApiKeyAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateDeviceRequestDto request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateDeviceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DeviceDto>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task ProvisionAsync(
        Guid deviceId,
        Guid tenantId,
        ProvisionDeviceRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DeviceHealthSummaryDto>
        GetHealthSummaryAsync(
            CancellationToken cancellationToken = default);

    Task<IEnumerable<OfflineDeviceDto>> GetOfflineDevicesAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SilentDeviceDto>>
        GetSilentDevicesAsync(
            CancellationToken cancellationToken = default);
}