using Dapper;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class MaterializationStateRepository
    : IMaterializationStateRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MaterializationStateRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MaterializationState?> GetByProcessAsync(
        string processName)
    {
        const string sql = """
            SELECT *
            FROM MaterializationStates
            WHERE ProcessName = @ProcessName
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<MaterializationState>(
            sql,
            new
            {
                ProcessName = processName
            });
    }

    public async Task<long> CreateAsync(
        MaterializationState state)
    {
        const string sql = """
            INSERT INTO MaterializationStates
            (
                ProcessName,
                LastProcessedUtc,
                UpdatedAtUtc
            )
            VALUES
            (
                @ProcessName,
                @LastProcessedUtc,
                @UpdatedAtUtc
            );

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            state);
    }

    public async Task UpdateAsync(
        MaterializationState state)
    {
        const string sql = """
            UPDATE MaterializationStates
            SET
                LastProcessedUtc = @LastProcessedUtc,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            state);
    }
}