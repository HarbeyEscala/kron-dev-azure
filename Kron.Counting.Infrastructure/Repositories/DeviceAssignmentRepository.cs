using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Infrastructure.Data;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class DeviceAssignmentRepository
    : IDeviceAssignmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeviceAssignmentRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> CreateAsync(
        DeviceAssignment assignment)
    {
        const string sql =
            """
            INSERT INTO dbo.DeviceAssignments
            (
                Id,
                DeviceId,
                MeasurementPointId,
                AssignedAtUtc,
                UnassignedAtUtc,
                BaselineTotalIn,
                BaselineTotalOut,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @DeviceId,
                @MeasurementPointId,
                @AssignedAtUtc,
                @UnassignedAtUtc,
                @BaselineTotalIn,
                @BaselineTotalOut,
                @CreatedAtUtc
            )
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, assignment);

        return assignment.Id;
    }

    public async Task<DeviceAssignment?> GetActiveAssignmentAsync(
        Guid deviceId)
    {
        const string sql =
            """
            SELECT TOP 1 *
            FROM dbo.DeviceAssignments
            WHERE DeviceId = @DeviceId
            AND UnassignedAtUtc IS NULL
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<DeviceAssignment>(
            sql,
            new { DeviceId = deviceId });
    }

    public async Task CloseAssignmentAsync(
        Guid assignmentId)
    {
        const string sql =
            """
            UPDATE dbo.DeviceAssignments
            SET UnassignedAtUtc = @UtcNow
            WHERE Id = @AssignmentId
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                AssignmentId = assignmentId,
                UtcNow = DateTime.UtcNow
            });
    }

    public async Task<IReadOnlyList<DeviceAssignment>> GetByDeviceAsync(
        Guid deviceId)
    {
        const string sql =
            """
            SELECT *
            FROM dbo.DeviceAssignments
            WHERE DeviceId = @DeviceId
            ORDER BY AssignedAtUtc DESC
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<DeviceAssignment>(
                sql,
                new { DeviceId = deviceId });

        return result.ToList();
    }

    public async Task TransferAsync(
    Guid deviceId,
    Guid newMeasurementPointId,
    int baselineTotalIn,
    int baselineTotalOut)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string closeSql =
                """
            UPDATE dbo.DeviceAssignments
            SET UnassignedAtUtc = @UtcNow
            WHERE DeviceId = @DeviceId
            AND UnassignedAtUtc IS NULL
            """;

            await connection.ExecuteAsync(
                closeSql,
                new
                {
                    DeviceId = deviceId,
                    UtcNow = DateTime.UtcNow
                },
                transaction);

            const string insertSql =
                """
            INSERT INTO dbo.DeviceAssignments
            (
                Id,
                DeviceId,
                MeasurementPointId,
                AssignedAtUtc,
                UnassignedAtUtc,
                BaselineTotalIn,
                BaselineTotalOut,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @DeviceId,
                @MeasurementPointId,
                @AssignedAtUtc,
                NULL,
                @BaselineTotalIn,
                @BaselineTotalOut,
                @CreatedAtUtc
            )
            """;

            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    Id = Guid.NewGuid(),
                    DeviceId = deviceId,
                    MeasurementPointId = newMeasurementPointId,
                    AssignedAtUtc = DateTime.UtcNow,
                    BaselineTotalIn = baselineTotalIn,
                    BaselineTotalOut = baselineTotalOut,
                    CreatedAtUtc = DateTime.UtcNow
                },
                transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<DeviceAssignment?> GetActiveAtAsync(
    Guid deviceId,
    DateTime timestampUtc)
    {
        const string sql =
            """
        SELECT TOP 1 *
        FROM dbo.DeviceAssignments
        WHERE DeviceId = @DeviceId
        AND AssignedAtUtc <= @TimestampUtc
        AND
        (
            UnassignedAtUtc IS NULL
            OR
            UnassignedAtUtc > @TimestampUtc
        )
        ORDER BY AssignedAtUtc DESC
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<DeviceAssignment>(
            sql,
            new
            {
                DeviceId = deviceId,
                TimestampUtc = timestampUtc
            });
    }
}