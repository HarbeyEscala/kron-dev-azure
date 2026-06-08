using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
//[Authorize(Roles = "Admin,Manager")]
[Route("api/v1/stores")]
public sealed class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("by-tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenantId(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _storeService.GetByTenantIdAsync(tenantId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storeService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStoreRequestDto request,
        CancellationToken cancellationToken)
    {
        var id = await _storeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStoreRequestDto request,
        CancellationToken cancellationToken)
    {
        await _storeService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _storeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}