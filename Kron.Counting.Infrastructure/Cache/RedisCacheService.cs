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
        _database = multiplexer.GetDatabase();
        _settings = settings.Value;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(
                value!.ToString());
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            await _database.StringSetAsync(
                key,
                json,
                expiration
                    ?? TimeSpan.FromMinutes(
                        _settings.DefaultExpirationMinutes));
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
        }
    }

    public async Task RemoveByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoints = _multiplexer.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                var server = _multiplexer.GetServer(endpoint);

                var keys = server.Keys(
                    pattern: pattern);

                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
        }
    }

    private static bool IsRedisFailure(Exception ex) =>
        ex is RedisConnectionException
        || ex is RedisTimeoutException
        || ex is RedisServerException
        || ex is ObjectDisposedException
        || ex is InvalidOperationException;
}
