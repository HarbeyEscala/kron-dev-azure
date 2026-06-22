namespace Kron.Counting.Domain.Entities;

public sealed class DeviceTelemetryStatus
{
    public Guid DeviceId { get; set; }

    public DateTime? LastSeenAtUtc { get; set; }

    public DateTime? LastPayloadUtc { get; set; }

    public string? IpAddress { get; set; }

    public bool IsOnline { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}