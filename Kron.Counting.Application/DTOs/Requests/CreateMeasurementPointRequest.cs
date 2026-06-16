namespace Kron.Counting.Application.DTOs;

public sealed class CreateMeasurementPointRequest
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }
}