using System.Text.Json;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Shared.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kron.Counting.Infrastructure.Cache;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly RedisSettings _settings;
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisCacheService(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisSettings> settings)
    {
        _multiplexer = multiplexer;

        _database =
            multiplexer.GetDatabase();

        _settings =
            settings.Value;
    }

    public async Task<T?> GetAsync<T>(
        string key,
    CancellationToken cancellationToken = default)
    {
        var value =
            await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(
            value!.ToString());
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var json =
            JsonSerializer.Serialize(value);

        await _database.StringSetAsync(
            key,
            json,
            expiration
                ?? TimeSpan.FromMinutes(
                    _settings.DefaultExpirationMinutes));
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(
        string pattern,
    CancellationToken cancellationToken = default)
    {
        var endpoints =
            _multiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server =
                _multiplexer.GetServer(endpoint);

            var keys =
                server.Keys(
                    pattern: pattern);

            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }
}