using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface ITelemetryDeviceResolver
{
    Task<Device?> ResolveByApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default);
}
