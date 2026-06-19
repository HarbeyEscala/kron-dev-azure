namespace Kron.Counting.Application.DTOs.Responses;

public sealed class DeviceApiKeyDto
{
    public Guid DeviceId { get; set; }

    public string ApiKey { get; set; } = string.Empty;
}
