namespace Kron.Counting.Application.DTOs.Responses;

public sealed class OfflineDeviceDto
{
    public Guid DeviceId { get; set; }

    public string? Name { get; set; }

    public string? SerialNumber { get; set; }

    public string? IpAddress { get; set; }

    public string? FirmwareVersion { get; set; }

    public DateTime? LastSeenAtUtc { get; set; }

    public int MinutesOffline { get; set; }
}