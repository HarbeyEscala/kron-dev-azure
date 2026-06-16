using Kron.Counting.Application.DTOs;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceAssignmentResolver
{
    Task<DeviceAssignmentContext?> ResolveAsync(
        Guid deviceId,
        DateTime timestampUtc,
        CancellationToken cancellationToken = default);
}