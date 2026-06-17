using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class StoreRepository : IStoreRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StoreRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Store>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                Code,
                Name,
                Description,
                Country,
                State,
                City,
                PostalCode,
                AddressLine1,
                AddressLine2,
                Latitude,
                Longitude,
                TimeZone,
                ContactName,
                ContactEmail,
                ContactPhone,
                Capacity,
                StoreType,
                Region,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Stores
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
            ORDER BY Name;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Store>(
            sql,
            new { TenantId = tenantId });
    }

    public async Task<Store?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                Code,
                Name,
                Description,
                Country,
                State,
                City,
                PostalCode,
                AddressLine1,
                AddressLine2,
                Latitude,
                Longitude,
                TimeZone,
                ContactName,
                ContactEmail,
                ContactPhone,
                Capacity,
                StoreType,
                Region,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Stores
            WHERE Id = @Id
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Store>(
            sql,
            new { Id = id });
    }

    public async Task<Store?> GetByCodeAsync(
        Guid tenantId,
        string code,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                Code,
                Name,
                Description,
                Country,
                State,
                City,
                PostalCode,
                AddressLine1,
                AddressLine2,
                Latitude,
                Longitude,
                TimeZone,
                ContactName,
                ContactEmail,
                ContactPhone,
                Capacity,
                StoreType,
                Region,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Stores
            WHERE TenantId = @TenantId
              AND Code = @Code
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Store>(
            sql,
            new
            {
                TenantId = tenantId,
                Code = code
            });
    }

    public async Task<Guid> CreateAsync(
        Store store,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Stores
            (
                Id,
                TenantId,
                Code,
                Name,
                Description,
                Country,
                State,
                City,
                PostalCode,
                AddressLine1,
                AddressLine2,
                Latitude,
                Longitude,
                TimeZone,
                ContactName,
                ContactEmail,
                ContactPhone,
                Capacity,
                StoreType,
                Region,
                IsActive,
                IsDeleted,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @TenantId,
                @Code,
                @Name,
                @Description,
                @Country,
                @State,
                @City,
                @PostalCode,
                @AddressLine1,
                @AddressLine2,
                @Latitude,
                @Longitude,
                @TimeZone,
                @ContactName,
                @ContactEmail,
                @ContactPhone,
                @Capacity,
                @StoreType,
                @Region,
                @IsActive,
                @IsDeleted,
                @CreatedAtUtc
            );
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, store);

        return store.Id;
    }

    public async Task UpdateAsync(
        Store store,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Stores
            SET
                Name = @Name,
                Description = @Description,
                Country = @Country,
                State = @State,
                City = @City,
                PostalCode = @PostalCode,
                AddressLine1 = @AddressLine1,
                AddressLine2 = @AddressLine2,
                Latitude = @Latitude,
                Longitude = @Longitude,
                TimeZone = @TimeZone,
                ContactName = @ContactName,
                ContactEmail = @ContactEmail,
                ContactPhone = @ContactPhone,
                Capacity = @Capacity,
                StoreType = @StoreType,
                Region = @Region,
                IsActive = @IsActive,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, store);
    }

    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Stores
            SET
                IsDeleted = 1,
                IsActive = 0,
                DeletedAtUtc = @DeletedAtUtc
            WHERE Id = @Id
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                DeletedAtUtc = DateTime.UtcNow
            });
    }
}