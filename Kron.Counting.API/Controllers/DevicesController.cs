using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet("by-store/{storeId:guid}")]
    public async Task<IActionResult> GetByStoreId(
        Guid storeId,
        CancellationToken cancellationToken)
    {
        var result = await _deviceService.GetByStoreIdAsync(storeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deviceService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeviceRequestDto request,
        CancellationToken cancellationToken)
    {
        var id = await _deviceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDeviceRequestDto request,
        CancellationToken cancellationToken)
    {
        await _deviceService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
