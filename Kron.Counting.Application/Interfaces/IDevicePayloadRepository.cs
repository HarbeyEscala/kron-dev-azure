using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IDevicePayloadRepository
{
    Task InsertAsync(DevicePayload payload);

    Task UpdateStatusAsync(
        Guid payloadId,
        string status,
        string? errorMessage = null);

    Task<DevicePayload?> GetByIdAsync(Guid id);

    Task<IEnumerable<DevicePayload>> GetFailedAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    Task IncrementRetryAsync(
    Guid id,
    CancellationToken cancellationToken = default);
}