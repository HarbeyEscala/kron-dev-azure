using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Domain.Constants;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class DevicePayloadRepository : IDevicePayloadRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DevicePayloadRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(DevicePayload payload)
    {
        const string sql = """
            INSERT INTO dbo.DevicePayloads
            (
                Id,
                DeviceId,
                PayloadType,
                RawHex,
                Status,
                ErrorMessage,
                ReceivedAtUtc,
                ProcessedAtUtc
            )
            VALUES
            (
                @Id,
                @DeviceId,
                @PayloadType,
                @RawHex,
                @Status,
                @ErrorMessage,
                @ReceivedAtUtc,
                @ProcessedAtUtc
            );
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, payload);
    }

    public async Task UpdateStatusAsync(
        Guid payloadId,
        string status,
        string? errorMessage = null)
    {
        const string sql = """
            UPDATE dbo.DevicePayloads
            SET
                Status = @Status,
                ErrorMessage = @ErrorMessage,
                ProcessedAtUtc = @ProcessedAtUtc
            WHERE Id = @PayloadId;
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                PayloadId = payloadId,
                Status = status,
                ErrorMessage = errorMessage,
                ProcessedAtUtc = DateTime.UtcNow
            });
    }

    public async Task<DevicePayload?> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT
                Id,
                DeviceId,
                PayloadType,
                RawHex,
                Status,
                ErrorMessage,
                ReceivedAtUtc,
                ProcessedAtUtc
            FROM dbo.DevicePayloads
            WHERE Id = @Id;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<DevicePayload>(
            sql,
            new { Id = id });
    }

    public async Task<IEnumerable<DevicePayload>> GetFailedAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT TOP (@Take)
                *
            FROM dbo.DevicePayloads
            WHERE Status = @Status
            ORDER BY ReceivedAtUtc;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QueryAsync<DevicePayload>(
                sql,
                new
                {
                    Take = take,
                    Status = PayloadStatuses.Failed
                });
        }

    public async Task IncrementRetryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            const string sql = """
            UPDATE dbo.DevicePayloads
            SET
                RetryCount = RetryCount + 1,
                LastRetryAtUtc = @Now
            WHERE Id = @Id;
        """;

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    Now = DateTime.UtcNow
                });
        }
}