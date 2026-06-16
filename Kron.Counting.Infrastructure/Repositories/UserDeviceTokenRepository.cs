using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Infrastructure.Data;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class UserDeviceTokenRepository
    : IUserDeviceTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserDeviceTokenRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RegisterAsync(
        UserDeviceToken token)
    {
        const string sql =
            """
            MERGE dbo.UserDeviceTokens AS target
            USING
            (
                SELECT
                    @Token AS Token
            ) AS source
            ON target.Token = source.Token

            WHEN MATCHED THEN
                UPDATE SET
                    UserId = @UserId,
                    TenantId = @TenantId,
                    Platform = @Platform,
                    IsActive = 1,
                    LastUsedAtUtc = @UtcNow

            WHEN NOT MATCHED THEN
                INSERT
                (
                    Id,
                    TenantId,
                    UserId,
                    Token,
                    Platform,
                    IsActive,
                    CreatedAtUtc,
                    LastUsedAtUtc
                )
                VALUES
                (
                    @Id,
                    @TenantId,
                    @UserId,
                    @Token,
                    @Platform,
                    1,
                    @UtcNow,
                    @UtcNow
                );
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                token.Id,
                token.TenantId,
                token.UserId,
                token.Token,
                token.Platform,
                UtcNow = DateTime.UtcNow
            });
    }

    public async Task<IReadOnlyList<UserDeviceToken>>
        GetActiveByTenantAsync(
            Guid tenantId)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.UserDeviceTokens
            WHERE TenantId = @TenantId
              AND IsActive = 1
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<UserDeviceToken>(
                sql,
                new { TenantId = tenantId });

        return result.ToList();
    }

    public async Task<IReadOnlyList<UserDeviceToken>>
        GetActiveByUserAsync(
            Guid userId)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.UserDeviceTokens
            WHERE UserId = @UserId
              AND IsActive = 1
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<UserDeviceToken>(
                sql,
                new { UserId = userId });

        return result.ToList();
    }

    public async Task DeactivateAsync(
        string token)
    {
        const string sql =
            """
            UPDATE dbo.UserDeviceTokens
            SET IsActive = 0
            WHERE Token = @Token
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new { Token = token });
    }

    public async Task TouchAsync(
        string token)
    {
        const string sql =
            """
            UPDATE dbo.UserDeviceTokens
            SET LastUsedAtUtc = @UtcNow
            WHERE Token = @Token
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                Token = token,
                UtcNow = DateTime.UtcNow
            });
    }
}