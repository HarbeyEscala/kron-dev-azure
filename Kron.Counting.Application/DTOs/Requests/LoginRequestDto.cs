namespace Kron.Counting.Application.DTOs.Requests;

public sealed class LoginRequestDto
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

}