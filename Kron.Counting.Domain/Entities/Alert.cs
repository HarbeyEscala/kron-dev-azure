namespace Kron.Counting.Domain.Entities;

public sealed class Alert
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? DeviceId { get; set; }

    public Guid? StoreId { get; set; }

    public Guid? MeasurementPointId { get; set; }

    public string Source { get; set; } = default!;

    public string Type { get; set; } = default!;

    public string Severity { get; set; } = default!;

    public string Message { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastTriggeredAtUtc { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public bool IsResolved { get; set; }
}