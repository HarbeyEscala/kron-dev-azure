using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/device-assignments")]
public sealed class DeviceAssignmentsController : ControllerBase
{
    private readonly IDeviceAssignmentRepository _deviceAssignmentRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMeasurementPointRepository _measurementPointRepository;

    public DeviceAssignmentsController(
        IDeviceAssignmentRepository deviceAssignmentRepository,
        IDeviceRepository deviceRepository,
        IMeasurementPointRepository measurementPointRepository)
    {
        _deviceAssignmentRepository = deviceAssignmentRepository;
        _deviceRepository = deviceRepository;
        _measurementPointRepository = measurementPointRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeviceAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                request.DeviceId,
                cancellationToken);

        if (device is null)
        {
            return NotFound("Device not found.");
        }

        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(
                request.MeasurementPointId);

        if (measurementPoint is null)
        {
            return NotFound("Measurement point not found.");
        }

        var activeAssignment =
            await _deviceAssignmentRepository.GetActiveAssignmentAsync(
                request.DeviceId);

        if (activeAssignment is not null)
        {
            return Conflict("Device already has an active assignment.");
        }

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

        return Created(
            $"/api/v1/device-assignments/{assignment.Id}",
            assignment);
    }

    [HttpGet("active/by-device/{deviceId:guid}")]
    public async Task<IActionResult> GetActiveByDevice(Guid deviceId)
    {
        var assignment =
            await _deviceAssignmentRepository.GetActiveAssignmentAsync(deviceId);

        if (assignment is null)
        {
            return NotFound();
        }

        return Ok(assignment);
    }

    [HttpGet("by-device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(Guid deviceId)
    {
        var assignments =
            await _deviceAssignmentRepository.GetByDeviceAsync(deviceId);

        return Ok(assignments);
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> Transfer(
        Guid id,
        [FromBody] TransferDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (device is null)
        {
            return NotFound();
        }

        await _deviceAssignmentRepository.TransferAsync(
            id,
            request.MeasurementPointId,
            device.LastTotalIn,
            device.LastTotalOut);

        return NoContent();
    }
}