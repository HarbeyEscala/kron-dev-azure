namespace Kron.Counting.Application.DTOs.Requests;

public sealed class CreateDeviceRequestDto
{
    public Guid? StoreId { get; set; }

    public string SerialNumber { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string DeviceType { get; set; } = default!;

    public string? FirmwareVersion { get; set; }
}