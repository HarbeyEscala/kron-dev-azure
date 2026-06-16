namespace Kron.Counting.Application.DTOs.Requests;

public sealed class RegisterDeviceTokenRequestDto
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Token { get; set; } = default!;

    public string Platform { get; set; } = default!;
}