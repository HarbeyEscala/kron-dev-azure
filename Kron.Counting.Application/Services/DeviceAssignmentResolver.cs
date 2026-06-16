using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Application.Services;

public sealed class DeviceAssignmentResolver
    : IDeviceAssignmentResolver
{
    private readonly IDeviceAssignmentRepository _deviceAssignmentRepository;
    private readonly IMeasurementPointRepository _measurementPointRepository;

    public DeviceAssignmentResolver(
        IDeviceAssignmentRepository deviceAssignmentRepository,
        IMeasurementPointRepository measurementPointRepository)
    {
        _deviceAssignmentRepository = deviceAssignmentRepository;
        _measurementPointRepository = measurementPointRepository;
    }

    public async Task<DeviceAssignmentContext?> ResolveAsync(
        Guid deviceId,
        DateTime timestampUtc,
        CancellationToken cancellationToken = default)
    {
        var assignment =
            await _deviceAssignmentRepository.GetActiveAtAsync(
                deviceId,
                timestampUtc);

        if (assignment is null)
        {
            return null;
        }

        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(
                assignment.MeasurementPointId);

        if (measurementPoint is null)
        {
            return null;
        }

        return new DeviceAssignmentContext
        {
            DeviceAssignmentId = assignment.Id,
            MeasurementPointId = measurementPoint.Id,
            StoreId = measurementPoint.StoreId
        };
    }
}