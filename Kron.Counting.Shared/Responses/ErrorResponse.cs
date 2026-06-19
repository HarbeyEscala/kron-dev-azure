namespace Kron.Counting.Shared.Responses;

public class ErrorResponse
{
    public bool Success { get; set; } = false;

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? ErrorCode { get; set; }

    public string? TraceId { get; set; }

    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
