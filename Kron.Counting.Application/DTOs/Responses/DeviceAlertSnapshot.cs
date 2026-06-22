public sealed class DeviceAlertSnapshot
{
    public Guid DeviceId { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastSeenAtUtc { get; set; }

    public DateTime? LastPayloadUtc { get; set; }
}