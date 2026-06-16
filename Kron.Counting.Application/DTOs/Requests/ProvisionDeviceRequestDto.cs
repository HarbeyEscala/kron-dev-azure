namespace Kron.Counting.Application.DTOs.Requests;

public sealed class ProvisionDeviceRequestDto
{
    public Guid MeasurementPointId { get; set; }

    public string Name { get; set; } = default!;
}