using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Application.Services;

public sealed class DeviceAssignmentResolver
    : IDeviceAssignmentResolver
{
    private readonly IDeviceAssignmentRepository _deviceAssignmentRepository;

    public DeviceAssignmentResolver(
        IDeviceAssignmentRepository deviceAssignmentRepository)
    {
        _deviceAssignmentRepository = deviceAssignmentRepository;
    }

    public async Task<DeviceAssignmentContext?> ResolveAsync(
        Guid deviceId,
        DateTime timestampUtc,
        CancellationToken cancellationToken = default)
    {
        var context =
            await _deviceAssignmentRepository.GetAssignmentContextAsync(
                deviceId,
                timestampUtc);

        if (context is null)
        {
            return null;
        }

        return new DeviceAssignmentContext
        {
            DeviceAssignmentId = context.DeviceAssignmentId,
            MeasurementPointId = context.MeasurementPointId,
            StoreId = context.StoreId
        };
    }
}