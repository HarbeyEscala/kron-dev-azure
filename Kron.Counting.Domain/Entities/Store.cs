namespace Kron.Counting.Domain.Entities;

public sealed class Store
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public string? StoreType { get; set; }

    public string? Region { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public Tenant? Tenant { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();

    public ICollection<MeasurementPoint> MeasurementPoints { get; set; }
        = new List<MeasurementPoint>();
}