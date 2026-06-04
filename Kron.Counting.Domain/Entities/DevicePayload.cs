namespace Kron.Counting.Domain.Entities;

public class DevicePayload
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public string PayloadType { get; set; } = string.Empty;

    public string RawHex { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public DateTime? LastRetryAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }
}