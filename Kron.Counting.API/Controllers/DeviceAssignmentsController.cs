using Kron.Counting.Application.DTOs;
using Kron.Counting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/device-assignments")]
public sealed class DeviceAssignmentsController : ControllerBase
{
    private readonly IDeviceAssignmentService _deviceAssignmentService;

    public DeviceAssignmentsController(
        IDeviceAssignmentService deviceAssignmentService)
    {
        _deviceAssignmentService = deviceAssignmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeviceAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _deviceAssignmentService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetActiveByDevice),
            new { deviceId = result.DeviceId },
            result);
    }

    [HttpGet("active/by-device/{deviceId:guid}")]
    public async Task<IActionResult> GetActiveByDevice(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var result =
            await _deviceAssignmentService.GetActiveByDeviceAsync(
                deviceId,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("by-device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var result =
            await _deviceAssignmentService.GetByDeviceAsync(
                deviceId,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> Transfer(
        Guid id,
        [FromBody] TransferDeviceRequest request,
        CancellationToken cancellationToken)
    {
        await _deviceAssignmentService.TransferAsync(
            id,
            request,
            cancellationToken);

        return NoContent();
    }
}
