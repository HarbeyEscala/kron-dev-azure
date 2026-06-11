namespace Kron.Counting.Application.DTOs.Requests;
public sealed class UpdateDeviceRequestDto
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = default!;

    public string DeviceType { get; set; } = default!;

    public string? FirmwareVersion { get; set; }

    public bool IsActive { get; set; }
}