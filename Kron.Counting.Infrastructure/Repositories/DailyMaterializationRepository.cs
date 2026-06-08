using Dapper;
using Kron.Counting.Application.DTOs.Internal;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class DailyMaterializationRepository
    : IDailyMaterializationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DailyMaterializationRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DailyMetricAggregationDto>> GetDailyAggregationsAsync(
        DateOnly fromDate,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                StoreId,
                CAST(CAST(MetricDate AS date) AS datetime2) AS MetricDate,
                SUM(PeopleIn) AS PeopleIn,
                SUM(PeopleOut) AS PeopleOut,
                MAX(PeakOccupancy) AS PeakOccupancy,
                CAST(AVG(CAST(AvgOccupancy AS decimal(18,2))) AS decimal(18,2)) AS AvgOccupancy
            FROM dbo.StoreHourlyMetrics
            WHERE MetricDate >= @FromDate
            GROUP BY
                StoreId,
                MetricDate
            ORDER BY
                MetricDate;
        """;

        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<DailyMetricAggregationSqlRow>(
            sql,
            new
            {
                FromDate = fromDate.ToDateTime(TimeOnly.MinValue)
            });

        return rows.Select(x => new DailyMetricAggregationDto
        {
            StoreId = x.StoreId,
            MetricDate = DateOnly.FromDateTime(x.MetricDate),
            PeopleIn = x.PeopleIn,
            PeopleOut = x.PeopleOut,
            PeakOccupancy = x.PeakOccupancy,
            AvgOccupancy = x.AvgOccupancy
        });
    }

    public async Task UpsertDailyMetricsAsync(
        IEnumerable<DailyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            MERGE dbo.StoreDailyMetrics AS target
            USING
            (
                SELECT
                    @StoreId AS StoreId,
                    @MetricDate AS MetricDate,
                    @PeopleIn AS PeopleIn,
                    @PeopleOut AS PeopleOut,
                    @PeakOccupancy AS PeakOccupancy,
                    @AvgOccupancy AS AvgOccupancy
            ) AS source
            ON target.StoreId = source.StoreId
               AND target.MetricDate = source.MetricDate

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
                    MetricDate,
                    PeopleIn,
                    PeopleOut,
                    PeakOccupancy,
                    AvgOccupancy,
                    CreatedAtUtc
                )
                VALUES
                (
                    source.StoreId,
                    source.MetricDate,
                    source.PeopleIn,
                    source.PeopleOut,
                    source.PeakOccupancy,
                    source.AvgOccupancy,
                    SYSUTCDATETIME()
                );
        """;

        using var connection = _connectionFactory.CreateConnection();

        foreach (var metric in metrics)
        {
            await connection.ExecuteAsync(
                sql,
                new
                {
                    metric.StoreId,
                    MetricDate = metric.MetricDate.ToDateTime(TimeOnly.MinValue),
                    metric.PeopleIn,
                    metric.PeopleOut,
                    metric.PeakOccupancy,
                    metric.AvgOccupancy
                });
        }
    }

    private sealed class DailyMetricAggregationSqlRow
    {
        public Guid StoreId { get; set; }

        public DateTime MetricDate { get; set; }

        public int PeopleIn { get; set; }

        public int PeopleOut { get; set; }

        public int PeakOccupancy { get; set; }

        public decimal AvgOccupancy { get; set; }
    }
}