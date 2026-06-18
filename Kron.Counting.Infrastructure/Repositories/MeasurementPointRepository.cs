using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Infrastructure.Data;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class MeasurementPointRepository
    : IMeasurementPointRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MeasurementPointRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateAsync(
        MeasurementPoint measurementPoint,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO dbo.MeasurementPoints
            (
                Id,
                StoreId,
                Name,
                Description,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            )
            VALUES
            (
                @Id,
                @StoreId,
                @Name,
                @Description,
                @IsActive,
                @CreatedAtUtc,
                @UpdatedAtUtc
            )
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, measurementPoint);

        return measurementPoint.Id;
    }

    public async Task<MeasurementPoint?> GetByIdAsync(Guid id)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.MeasurementPoints
            WHERE Id = @Id
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<MeasurementPoint>(
            sql,
            new { Id = id });
    }

    public async Task<IReadOnlyList<MeasurementPoint>> GetByStoreAsync(
        Guid storeId)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.MeasurementPoints
            WHERE StoreId = @StoreId
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<MeasurementPoint>(
                sql,
                new { StoreId = storeId });

        return result.ToList();
    }
}