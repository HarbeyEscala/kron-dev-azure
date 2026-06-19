using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IUserDeviceTokenRepository _tokenRepository;
    private readonly IFirebaseNotificationService _firebaseNotificationService;

    public NotificationsController(
        IUserDeviceTokenRepository tokenRepository,
        IFirebaseNotificationService firebaseNotificationService)
    {
        _tokenRepository = tokenRepository;
        _firebaseNotificationService =
            firebaseNotificationService;
    }

    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken(
        [FromBody] RegisterDeviceTokenRequestDto request)
    {
        var entity = new UserDeviceToken
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TenantId = request.TenantId,
            Token = request.Token.Trim(),
            Platform = request.Platform.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastUsedAtUtc = DateTime.UtcNow
        };

        await _tokenRepository.RegisterAsync(entity);

        return NoContent();
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(
        [FromBody] TestNotificationRequestDto request)
    {
        var messageId =
            await _firebaseNotificationService
                .SendAsync(
                    request.Token,
                    request.Title,
                    request.Body);

        return Ok(new
        {
            MessageId = messageId
        });
    }
}
