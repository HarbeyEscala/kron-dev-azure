using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Cache;

public sealed class CacheInvalidationService
    : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationService(
        ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateAnalyticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(
                "analytics:*",
                cancellationToken);
        }
        catch
        {
        }
    }
}
