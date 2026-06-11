namespace Kron.Counting.Application.DTOs.Responses;

public sealed class LoginResponseDto
{
    public string AccessToken { get; set; } = default!;

    public int ExpiresIn { get; set; }

    public string TokenType { get; set; } = "Bearer";

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = default!;

    public string Role { get; set; } = default!;
}