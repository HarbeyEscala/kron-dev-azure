using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.DTOs.Responses;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceAssignmentService
{
    Task<DeviceAssignmentDto> CreateAsync(
        CreateDeviceAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<DeviceAssignmentDto> GetActiveByDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceAssignmentDto>> GetByDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default);

    Task TransferAsync(
        Guid deviceId,
        TransferDeviceRequest request,
        CancellationToken cancellationToken = default);
}
