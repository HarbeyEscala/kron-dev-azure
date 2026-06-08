using Dapper;
using Kron.Counting.Application.DTOs.Internal;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class HourlyMaterializationRepository
    : IHourlyMaterializationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HourlyMaterializationRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HourlyMetricAggregationDto>> GetHourlyAggregationsAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                d.StoreId,
                CAST(CAST(dr.ReadingTimestampUtc AS date) AS datetime2) AS MetricDate,
                CAST(DATEPART(HOUR, dr.ReadingTimestampUtc) AS tinyint) AS MetricHour,
                SUM(dr.PeopleIn) AS PeopleIn,
                SUM(dr.PeopleOut) AS PeopleOut,
                MAX(dr.Occupancy) AS PeakOccupancy,
                CAST(AVG(CAST(dr.Occupancy AS decimal(18,2))) AS decimal(18,2)) AS AvgOccupancy
            FROM dbo.DeviceReadings dr
            INNER JOIN dbo.Devices d
                ON d.Id = dr.DeviceId
            WHERE dr.ReadingTimestampUtc > @FromUtc
              AND d.IsDeleted = 0
              AND d.IsActive = 1
            GROUP BY
                d.StoreId,
                CAST(dr.ReadingTimestampUtc AS date),
                DATEPART(HOUR, dr.ReadingTimestampUtc)
            ORDER BY
                MetricDate,
                MetricHour;
        """;

        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<HourlyMetricAggregationSqlRow>(
            sql,
            new { FromUtc = fromUtc });

        return rows.Select(x => new HourlyMetricAggregationDto
        {
            StoreId = x.StoreId,
            MetricDate = DateOnly.FromDateTime(x.MetricDate),
            MetricHour = x.MetricHour,
            PeopleIn = x.PeopleIn,
            PeopleOut = x.PeopleOut,
            PeakOccupancy = x.PeakOccupancy,
            AvgOccupancy = x.AvgOccupancy
        });
    }

    public async Task UpsertHourlyMetricsAsync(
        IEnumerable<HourlyMetricAggregationDto> metrics,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            MERGE dbo.StoreHourlyMetrics AS target
            USING
            (
                SELECT
                    @StoreId AS StoreId,
                    @MetricDate AS MetricDate,
                    @MetricHour AS MetricHour,
                    @PeopleIn AS PeopleIn,
                    @PeopleOut AS PeopleOut,
                    @PeakOccupancy AS PeakOccupancy,
                    @AvgOccupancy AS AvgOccupancy
            ) AS source
            ON target.StoreId = source.StoreId
               AND target.MetricDate = source.MetricDate
               AND target.MetricHour = source.MetricHour

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
                    MetricHour,
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
                    source.MetricHour,
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
                    metric.MetricHour,
                    metric.PeopleIn,
                    metric.PeopleOut,
                    metric.PeakOccupancy,
                    metric.AvgOccupancy
                });
        }
    }

    private sealed class HourlyMetricAggregationSqlRow
    {
        public Guid StoreId { get; set; }

        public DateTime MetricDate { get; set; }

        public byte MetricHour { get; set; }

        public int PeopleIn { get; set; }

        public int PeopleOut { get; set; }

        public int PeakOccupancy { get; set; }

        public decimal AvgOccupancy { get; set; }
    }
}