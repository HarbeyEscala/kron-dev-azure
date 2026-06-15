namespace Kron.Counting.Application.DTOs.Responses;

public sealed class DeviceDto
{
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public string SerialNumber { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string DeviceType { get; set; } = default!;

    public string? FirmwareVersion { get; set; }

    public string? IpAddress { get; set; }

    public string ProvisioningStatus { get; set; } = "Pending";

    public DateTime? LastSeenAtUtc { get; set; }

    public bool IsOnline { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}