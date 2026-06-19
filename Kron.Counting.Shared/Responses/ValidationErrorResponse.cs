namespace Kron.Counting.Shared.Responses;

public sealed class ValidationErrorResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    public DateTime TimestampUtc { get; set; }
}
