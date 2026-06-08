namespace Kron.Counting.Domain.Entities;

public sealed class MaterializationState
{
    public long Id { get; set; }

    public string ProcessName { get; set; } = default!;

    public DateTime LastProcessedUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}