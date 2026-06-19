using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class TelemetryDeviceResolver : ITelemetryDeviceResolver
{
    private readonly IDeviceRepository _deviceRepository;

    public TelemetryDeviceResolver(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<Device?> ResolveByApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        return await _deviceRepository.GetByApiKeyAsync(
            apiKey,
            cancellationToken);
    }
}
