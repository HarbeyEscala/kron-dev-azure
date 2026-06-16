using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceAssignmentRepository
{
    Task<Guid> CreateAsync(DeviceAssignment assignment);

    Task<DeviceAssignment?> GetActiveAssignmentAsync(Guid deviceId);

    Task CloseAssignmentAsync(Guid assignmentId);

    Task<IReadOnlyList<DeviceAssignment>> GetByDeviceAsync(Guid deviceId);
    Task TransferAsync(
        Guid deviceId,
        Guid newMeasurementPointId,
        int baselineTotalIn,
        int baselineTotalOut);

    Task<DeviceAssignment?> GetActiveAtAsync(
        Guid deviceId,
        DateTime timestampUtc);
}