namespace Kron.Counting.Application.DTOs.Responses;

public sealed class SilentDeviceDto
{
    public Guid DeviceId { get; set; }

    public string? Name { get; set; }

    public string? SerialNumber { get; set; }

    public DateTime? LastPayloadUtc { get; set; }

    public int MinutesSilent { get; set; }
}