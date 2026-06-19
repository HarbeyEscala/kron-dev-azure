using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("by-tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenantId(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _userService.GetByTenantIdAsync(tenantId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            throw new NotFoundException("User not found.");

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var id = await _userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        await _userService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}