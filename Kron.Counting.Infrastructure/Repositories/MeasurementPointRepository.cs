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

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                measurementPoint,
                cancellationToken: cancellationToken));

        return measurementPoint.Id;
    }

    public async Task<MeasurementPoint?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
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
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<MeasurementPoint>> GetByStoreAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
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
                new CommandDefinition(
                    sql,
                    new { StoreId = storeId },
                    cancellationToken: cancellationToken));

        return result.ToList();
    }
}
