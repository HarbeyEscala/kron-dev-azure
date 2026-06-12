namespace Kron.Counting.Domain.Entities;

public sealed class Device
{
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public string ProvisioningStatus { get; set; } = "Pending";

    public string SerialNumber { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string DeviceType { get; set; } = default!;

    public string ApiKey { get; set; } = default!;

    public string? IpAddress { get; set; }

    public string? FirmwareVersion { get; set; }

    public DateTime? LastSeenAtUtc { get; set; }

    public bool IsOnline { get; set; }

    public bool IsActive { get; set; }

    public int LastTotalIn { get; set; }

    public int LastTotalOut { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Store? Store { get; set; }

    public ICollection<DeviceReading> Readings { get; set; } = new List<DeviceReading>();
}