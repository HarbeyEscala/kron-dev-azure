namespace Kron.Counting.Application.DTOs.Requests;

public sealed class ProvisionDeviceRequestDto
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;
}