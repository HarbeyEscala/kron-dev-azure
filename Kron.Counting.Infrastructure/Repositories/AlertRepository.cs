using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Infrastructure.Data;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class AlertRepository
    : IAlertRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AlertRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateAsync(
        Alert alert)
    {
        const string sql =
            """
            INSERT INTO dbo.Alerts
            (
                Id,
                TenantId,
                DeviceId,
                StoreId,
                MeasurementPointId,
                Source,
                Type,
                Severity,
                Message,
                CreatedAtUtc,
                LastTriggeredAtUtc,
                ResolvedAtUtc,
                IsResolved
            )
            VALUES
            (
                @Id,
                @TenantId,
                @DeviceId,
                @StoreId,
                @MeasurementPointId,
                @Source,
                @Type,
                @Severity,
                @Message,
                @CreatedAtUtc,
                @LastTriggeredAtUtc,
                @ResolvedAtUtc,
                @IsResolved
            )
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            alert);

        return alert.Id;
    }

    public async Task<Alert?> GetOpenAlertAsync(
        Guid tenantId,
        Guid? deviceId,
        string type)
    {
        const string sql =
            """
            SELECT TOP 1 *
            FROM dbo.Alerts
            WHERE TenantId = @TenantId
              AND DeviceId = @DeviceId
              AND Type = @Type
              AND IsResolved = 0
            ORDER BY CreatedAtUtc DESC
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Alert>(
            sql,
            new
            {
                TenantId = tenantId,
                DeviceId = deviceId,
                Type = type
            });
    }

    public async Task TouchAsync(
        Guid alertId)
    {
        const string sql =
            """
            UPDATE dbo.Alerts
            SET LastTriggeredAtUtc = @UtcNow
            WHERE Id = @AlertId
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                AlertId = alertId,
                UtcNow = DateTime.UtcNow
            });
    }

    public async Task ResolveAsync(
        Guid alertId)
    {
        const string sql =
            """
            UPDATE dbo.Alerts
            SET
                IsResolved = 1,
                ResolvedAtUtc = @UtcNow
            WHERE Id = @AlertId
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                AlertId = alertId,
                UtcNow = DateTime.UtcNow
            });
    }

    public async Task<IReadOnlyList<Alert>>
        GetOpenAlertsAsync(
            Guid tenantId)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.Alerts
            WHERE TenantId = @TenantId
              AND IsResolved = 0
            ORDER BY CreatedAtUtc DESC
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<Alert>(
                sql,
                new
                {
                    TenantId = tenantId
                });

        return result.ToList();
    }

    public async Task<IReadOnlyList<Alert>>
        GetHistoryAsync(
            Guid tenantId,
            int take = 100)
    {
        const string sql =
            """
        SELECT TOP (@Take) *
        FROM dbo.Alerts
        WHERE TenantId = @TenantId
        ORDER BY CreatedAtUtc DESC
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<Alert>(
                sql,
                new
                {
                    TenantId = tenantId,
                    Take = take
                });

        return result.ToList();
    }

    public async Task<IReadOnlyList<Alert>>
        GetByDeviceAsync(
            Guid tenantId,
            Guid deviceId)
    {
        const string sql =
            """
        SELECT *
        FROM dbo.Alerts
        WHERE TenantId = @TenantId
          AND DeviceId = @DeviceId
        ORDER BY CreatedAtUtc DESC
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<Alert>(
                sql,
                new
                {
                    TenantId = tenantId,
                    DeviceId = deviceId
                });

        return result.ToList();
    }

    public async Task<IReadOnlyList<Alert>>
        GetByStoreAsync(
            Guid tenantId,
            Guid storeId)
    {
        const string sql =
            """
        SELECT *
        FROM dbo.Alerts
        WHERE TenantId = @TenantId
          AND StoreId = @StoreId
        ORDER BY CreatedAtUtc DESC
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<Alert>(
                sql,
                new
                {
                    TenantId = tenantId,
                    StoreId = storeId
                });

        return result.ToList();
    }

    public async Task<Alert?> GetOpenStoreAlertAsync(
        Guid tenantId,
        Guid storeId,
        string type)
    {
        const string sql =
            """
        SELECT TOP 1 *
        FROM dbo.Alerts
        WHERE TenantId = @TenantId
          AND StoreId = @StoreId
          AND Type = @Type
          AND IsResolved = 0
        ORDER BY CreatedAtUtc DESC
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Alert>(
            sql,
            new
            {
                TenantId = tenantId,
                StoreId = storeId,
                Type = type
            });
    }
}