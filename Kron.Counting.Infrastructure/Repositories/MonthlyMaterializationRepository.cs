using Dapper;
using Kron.Counting.Application.DTOs.Internal;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class MonthlyMaterializationRepository
    : IMonthlyMaterializationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MonthlyMaterializationRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<MonthlyMetricAggregationDto>>
        GetMonthlyAggregationsAsync(
            DateOnly fromDate,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                StoreId,
                YEAR(MetricDate) AS MetricYear,
                MONTH(MetricDate) AS MetricMonth,
                SUM(PeopleIn) AS PeopleIn,
                SUM(PeopleOut) AS PeopleOut,
                MAX(PeakOccupancy) AS PeakOccupancy,
                AVG(AvgOccupancy) AS AvgOccupancy
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= @FromDate
            GROUP BY
                StoreId,
                YEAR(MetricDate),
                MONTH(MetricDate)
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<
            MonthlyMetricAggregationDto>(
            sql,
            new
            {
                FromDate =
                    fromDate.ToDateTime(TimeOnly.MinValue)
            });
    }

    public async Task UpsertMonthlyMetricsAsync(
        IEnumerable<MonthlyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        MERGE dbo.StoreMonthlyMetrics AS target
        USING
        (
            SELECT
                @StoreId AS StoreId,
                @MetricYear AS MetricYear,
                @MetricMonth AS MetricMonth,
                @PeopleIn AS PeopleIn,
                @PeopleOut AS PeopleOut,
                @PeakOccupancy AS PeakOccupancy,
                @AvgOccupancy AS AvgOccupancy
        ) source
        ON
            target.StoreId = source.StoreId
            AND target.MetricYear = source.MetricYear
            AND target.MetricMonth = source.MetricMonth

        WHEN MATCHED THEN
            UPDATE SET
                PeopleIn = source.PeopleIn,
                PeopleOut = source.PeopleOut,
                PeakOccupancy = source.PeakOccupancy,
                AvgOccupancy = source.AvgOccupancy,
                UpdatedAtUtc = SYSUTCDATETIME()

        WHEN NOT MATCHED THEN
            INSERT
            (
                StoreId,
                MetricYear,
                MetricMonth,
                PeopleIn,
                PeopleOut,
                PeakOccupancy,
                AvgOccupancy,
                CreatedAtUtc
            )
            VALUES
            (
                source.StoreId,
                source.MetricYear,
                source.MetricMonth,
                source.PeopleIn,
                source.PeopleOut,
                source.PeakOccupancy,
                source.AvgOccupancy,
                SYSUTCDATETIME()
            );
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        foreach (var metric in metrics)
        {
            await connection.ExecuteAsync(sql, metric);
        }
    }
}