namespace Kron.Counting.Application.DTOs.Requests;

public sealed class TestNotificationRequestDto
{
    public string Token { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;
}