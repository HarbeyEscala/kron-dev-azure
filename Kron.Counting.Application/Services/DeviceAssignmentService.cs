using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Exceptions;

namespace Kron.Counting.Application.Services;

public sealed class DeviceAssignmentService : IDeviceAssignmentService
{
    private readonly IDeviceAssignmentRepository _deviceAssignmentRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMeasurementPointRepository _measurementPointRepository;

    public DeviceAssignmentService(
        IDeviceAssignmentRepository deviceAssignmentRepository,
        IDeviceRepository deviceRepository,
        IMeasurementPointRepository measurementPointRepository)
    {
        _deviceAssignmentRepository = deviceAssignmentRepository;
        _deviceRepository = deviceRepository;
        _measurementPointRepository = measurementPointRepository;
    }

    public async Task<DeviceAssignmentDto> CreateAsync(
        CreateDeviceAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                request.DeviceId,
                cancellationToken);

        if (device is null)
            throw new NotFoundException("Device not found.");

        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(
                request.MeasurementPointId,
                cancellationToken);

        if (measurementPoint is null)
            throw new NotFoundException("Measurement point not found.");

        var activeAssignment =
            await _deviceAssignmentRepository.GetActiveAssignmentAsync(
                request.DeviceId);

        if (activeAssignment is not null)
            throw new ConflictException(
                "Device already has an active assignment.");

        var assignment = new DeviceAssignment
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            MeasurementPointId = request.MeasurementPointId,
            AssignedAtUtc = DateTime.UtcNow,
            UnassignedAtUtc = null,
            BaselineTotalIn = device.LastTotalIn,
            BaselineTotalOut = device.LastTotalOut,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _deviceAssignmentRepository.CreateAsync(assignment);

        return ToDto(assignment);
    }

    public async Task<DeviceAssignmentDto> GetActiveByDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var assignment =
            await _deviceAssignmentRepository.GetActiveAssignmentAsync(
                deviceId);

        if (assignment is null)
            throw new NotFoundException("Active assignment not found.");

        return ToDto(assignment);
    }

    public async Task<IReadOnlyList<DeviceAssignmentDto>> GetByDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var assignments =
            await _deviceAssignmentRepository.GetByDeviceAsync(deviceId);

        return assignments.Select(ToDto).ToList();
    }

    public async Task TransferAsync(
        Guid deviceId,
        TransferDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                deviceId,
                cancellationToken);

        if (device is null)
            throw new NotFoundException("Device not found.");

        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(
                request.MeasurementPointId,
                cancellationToken);

        if (measurementPoint is null)
            throw new NotFoundException("Measurement point not found.");

        await _deviceAssignmentRepository.TransferAsync(
            deviceId,
            request.MeasurementPointId,
            device.LastTotalIn,
            device.LastTotalOut);
    }

    private static DeviceAssignmentDto ToDto(DeviceAssignment assignment)
    {
        return new DeviceAssignmentDto
        {
            Id = assignment.Id,
            DeviceId = assignment.DeviceId,
            MeasurementPointId = assignment.MeasurementPointId,
            AssignedAtUtc = assignment.AssignedAtUtc,
            UnassignedAtUtc = assignment.UnassignedAtUtc,
            BaselineTotalIn = assignment.BaselineTotalIn,
            BaselineTotalOut = assignment.BaselineTotalOut,
            CreatedAtUtc = assignment.CreatedAtUtc
        };
    }
}
