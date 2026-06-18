using Dapper;
using Kron.Counting.Application.DTOs.Analytics;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnalyticsRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HourlyAnalyticsDto>> GetHourlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        SELECT
            DATEADD(
                HOUR,
                sh.MetricHour,
                CAST(sh.MetricDate AS datetime2)
            ) AS HourUtc,

            SUM(sh.PeopleIn) AS PeopleIn,

            SUM(sh.PeopleOut) AS PeopleOut

        FROM dbo.StoreHourlyMetrics sh

        WHERE sh.MetricDate >= CAST(@FromUtc AS DATE)
          AND sh.MetricDate <= CAST(@ToUtc AS DATE)

          AND (@StoreId IS NULL OR sh.StoreId = @StoreId)

        GROUP BY
            sh.MetricDate,
            sh.MetricHour

        ORDER BY HourUtc;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<HourlyAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<DailyAnalyticsDto>> GetDailyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        SELECT
            MetricDate AS DateUtc,

            SUM(PeopleIn) AS PeopleIn,

            SUM(PeopleOut) AS PeopleOut

        FROM dbo.StoreDailyMetrics

        WHERE MetricDate >= CAST(@FromUtc AS DATE)
          AND MetricDate <= CAST(@ToUtc AS DATE)

          AND (@StoreId IS NULL OR StoreId = @StoreId)

        GROUP BY MetricDate

        ORDER BY MetricDate;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<DailyAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<MonthlyAnalyticsDto>> GetMonthlyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        SELECT
            MetricYear AS Year,

            MetricMonth AS Month,

            SUM(PeopleIn) AS PeopleIn,

            SUM(PeopleOut) AS PeopleOut

        FROM dbo.StoreMonthlyMetrics

        WHERE
            (
                MetricYear > YEAR(@FromUtc)
                OR (
                    MetricYear = YEAR(@FromUtc)
                    AND MetricMonth >= MONTH(@FromUtc)
                )
            )
        AND
            (
                MetricYear < YEAR(@ToUtc)
                OR (
                    MetricYear = YEAR(@ToUtc)
                    AND MetricMonth <= MONTH(@ToUtc)
                )
            )

        AND (@StoreId IS NULL OR StoreId = @StoreId)

        GROUP BY
            MetricYear,
            MetricMonth

        ORDER BY
            MetricYear,
            MetricMonth;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<MonthlyAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<AnalyticsKpiDto> GetKpisAsync(
         Guid? storeId,
         Guid? deviceId,
         DateTime fromUtc,
         DateTime toUtc)
    {
        const string sql = """
        WITH HourlyTraffic AS
        (
            SELECT
                DATEADD(
                    HOUR,
                    MetricHour,
                    CAST(MetricDate AS datetime2)
                ) AS HourUtc,

                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreHourlyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY
                MetricDate,
                MetricHour
        ),
        DailyTraffic AS
        (
            SELECT
                MetricDate AS TrafficDate,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY
                MetricDate
        ),
        Totals AS
        (
            SELECT
                ISNULL(SUM(PeopleIn), 0) AS TotalVisitors,
                ISNULL(SUM(PeopleOut), 0) AS TotalExits
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
        )
        SELECT
            t.TotalVisitors,

            t.TotalExits,

            t.TotalVisitors - t.TotalExits AS NetTraffic,

            (
                SELECT TOP 1 HourUtc
                FROM HourlyTraffic
                ORDER BY Visitors DESC
            ) AS PeakHourUtc,

            ISNULL(
                (
                    SELECT TOP 1 Visitors
                    FROM HourlyTraffic
                    ORDER BY Visitors DESC
                ),
                0
            ) AS PeakHourVisitors,

            (
                SELECT TOP 1 TrafficDate
                FROM DailyTraffic
                ORDER BY Visitors DESC
            ) AS PeakDayUtc,

            ISNULL(
                (
                    SELECT TOP 1 Visitors
                    FROM DailyTraffic
                    ORDER BY Visitors DESC
                ),
                0
            ) AS PeakDayVisitors,

            ISNULL(
                (
                    SELECT AVG(CAST(Visitors AS DECIMAL(18,2)))
                    FROM DailyTraffic
                ),
                0
            ) AS AverageVisitorsPerDay,

            ISNULL(
                (
                    SELECT AVG(CAST(Visitors AS DECIMAL(18,2)))
                    FROM HourlyTraffic
                ),
                0
            ) AS AverageVisitorsPerHour

        FROM Totals t;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AnalyticsKpiDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            })
            ?? new AnalyticsKpiDto();
    }

    public async Task<GrowthAnalyticsDto> GetGrowthAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
    DECLARE @PeriodDays INT =
        DATEDIFF(DAY, @FromUtc, @ToUtc) + 1;

    DECLARE @PreviousFromUtc DATE =
        DATEADD(DAY, -@PeriodDays, CAST(@FromUtc AS DATE));

    DECLARE @PreviousToUtc DATE =
        DATEADD(DAY, -1, CAST(@FromUtc AS DATE));

    WITH CurrentPeriod AS
    (
        SELECT
            ISNULL(SUM(PeopleIn), 0) AS Visitors
        FROM dbo.StoreDailyMetrics
        WHERE MetricDate >= CAST(@FromUtc AS DATE)
          AND MetricDate <= CAST(@ToUtc AS DATE)
          AND (@StoreId IS NULL OR StoreId = @StoreId)
    ),
    PreviousPeriod AS
    (
        SELECT
            ISNULL(SUM(PeopleIn), 0) AS Visitors
        FROM dbo.StoreDailyMetrics
        WHERE MetricDate >= @PreviousFromUtc
          AND MetricDate <= @PreviousToUtc
          AND (@StoreId IS NULL OR StoreId = @StoreId)
    )

    SELECT

        c.Visitors AS CurrentPeriodVisitors,

        p.Visitors AS PreviousPeriodVisitors,

        CASE
            WHEN p.Visitors = 0 THEN 0
            ELSE
                CAST(
                    (
                        (CAST(c.Visitors AS DECIMAL(18,2))
                        - CAST(p.Visitors AS DECIMAL(18,2)))
                        /
                        CAST(p.Visitors AS DECIMAL(18,2))
                    ) * 100
                    AS DECIMAL(18,2)
                )
        END AS GrowthPercentage

    FROM CurrentPeriod c
    CROSS JOIN PreviousPeriod p;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<GrowthAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            })
            ?? new GrowthAnalyticsDto();
    }

    public async Task<OccupancyAnalyticsDto> GetOccupancyAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        SELECT

            MAX(PeakOccupancy) AS PeakOccupancy,

            NULL AS PeakOccupancyUtc,

            AVG(CAST(AvgOccupancy AS DECIMAL(18,2)))
                AS AverageOccupancy

        FROM dbo.StoreHourlyMetrics

        WHERE MetricDate >= CAST(@FromUtc AS DATE)
          AND MetricDate <= CAST(@ToUtc AS DATE)

          AND (@StoreId IS NULL OR StoreId = @StoreId);
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<
            OccupancyAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            })
            ?? new OccupancyAnalyticsDto();
    }

    public async Task<ComparisonAnalyticsDto> GetComparisonAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        DECLARE @PeriodDays INT =
            DATEDIFF(DAY, @FromUtc, @ToUtc) + 1;

        DECLARE @PreviousFromUtc DATE =
            DATEADD(DAY, -@PeriodDays, CAST(@FromUtc AS DATE));

        DECLARE @PreviousToUtc DATE =
            DATEADD(DAY, -1, CAST(@FromUtc AS DATE));

        WITH CurrentPeriod AS
        (
            SELECT
                ISNULL(SUM(PeopleIn), 0) AS Visitors,
                ISNULL(SUM(PeopleOut), 0) AS Exits
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
        ),
        PreviousPeriod AS
        (
            SELECT
                ISNULL(SUM(PeopleIn), 0) AS Visitors,
                ISNULL(SUM(PeopleOut), 0) AS Exits
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= @PreviousFromUtc
              AND MetricDate <= @PreviousToUtc
              AND (@StoreId IS NULL OR StoreId = @StoreId)
        )

        SELECT

            c.Visitors AS CurrentPeriodVisitors,

            p.Visitors AS PreviousPeriodVisitors,

            c.Exits AS CurrentPeriodExits,

            p.Exits AS PreviousPeriodExits,

            CASE
                WHEN p.Visitors = 0 THEN 0
                ELSE
                    CAST(
                        (
                            (CAST(c.Visitors AS DECIMAL(18,2))
                            - CAST(p.Visitors AS DECIMAL(18,2)))
                            /
                            CAST(p.Visitors AS DECIMAL(18,2))
                        ) * 100
                        AS DECIMAL(18,2)
                    )
            END AS VisitorGrowthPercentage,

            CASE
                WHEN p.Exits = 0 THEN 0
                ELSE
                    CAST(
                        (
                            (CAST(c.Exits AS DECIMAL(18,2))
                            - CAST(p.Exits AS DECIMAL(18,2)))
                            /
                            CAST(p.Exits AS DECIMAL(18,2))
                        ) * 100
                        AS DECIMAL(18,2)
                    )
            END AS ExitGrowthPercentage

        FROM CurrentPeriod c
        CROSS JOIN PreviousPeriod p;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<ComparisonAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            })
            ?? new ComparisonAnalyticsDto();
    }

    public async Task<IEnumerable<TrendAnalyticsDto>> GetTrendsAsync(
        Guid? storeId,
        Guid? deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
    SELECT

        MetricDate AS DateUtc,

        SUM(PeopleIn) AS Visitors,

        SUM(PeopleOut) AS Exits

    FROM dbo.StoreDailyMetrics

    WHERE MetricDate >= CAST(@FromUtc AS DATE)
      AND MetricDate <= CAST(@ToUtc AS DATE)

      AND (@StoreId IS NULL OR StoreId = @StoreId)

    GROUP BY
        MetricDate

    ORDER BY
        MetricDate;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<TrendAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<TopDayAnalyticsDto>> GetTopDaysAsync(
    Guid? storeId,
    Guid? deviceId,
    DateTime fromUtc,
    DateTime toUtc,
    int top = 10)
    {
        const string sql = """
    SELECT TOP (@Top)

        MetricDate AS DateUtc,

        SUM(PeopleIn) AS Visitors

    FROM dbo.StoreDailyMetrics

    WHERE MetricDate >= CAST(@FromUtc AS DATE)
      AND MetricDate <= CAST(@ToUtc AS DATE)

      AND (@StoreId IS NULL OR StoreId = @StoreId)

    GROUP BY
        MetricDate

    ORDER BY
        SUM(PeopleIn) DESC;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<TopDayAnalyticsDto>(
            sql,
            new
            {
                Top = top,
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<TopStoreAnalyticsDto>> GetTopStoresAsync(
        DateTime fromUtc,
        DateTime toUtc,
    int top = 10)
    {
        const string sql = """
            SELECT TOP (@Top)

                s.Id AS StoreId,

                s.Name AS StoreName,

                SUM(sd.PeopleIn) AS Visitors

            FROM dbo.StoreDailyMetrics sd

            INNER JOIN dbo.Stores s
                ON s.Id = sd.StoreId

            WHERE sd.MetricDate >= CAST(@FromUtc AS DATE)
              AND sd.MetricDate <= CAST(@ToUtc AS DATE)

              AND s.IsDeleted = 0

            GROUP BY
                s.Id,
                s.Name

            ORDER BY
                SUM(sd.PeopleIn) DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<TopStoreAnalyticsDto>(
            sql,
            new
            {
                Top = top,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<OccupancyTrendDto>> GetOccupancyTrendsAsync(
        Guid storeId,
        DateTime fromUtc,
        DateTime toUtc)     
    {
        const string sql = """
        SELECT

            DATEADD(
                HOUR,
                MetricHour,
                CAST(MetricDate AS datetime2)
            ) AS HourUtc,

            PeakOccupancy AS Occupancy

        FROM dbo.StoreHourlyMetrics

        WHERE StoreId = @StoreId

          AND MetricDate >= CAST(@FromUtc AS DATE)

          AND MetricDate <= CAST(@ToUtc AS DATE)

        ORDER BY
            MetricDate,
            MetricHour;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<
            OccupancyTrendDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<StoreComparisonAnalyticsDto>> GetStoreComparisonAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int top = 10)
    {
            const string sql = """
        SELECT TOP (@Top)

            s.Id AS StoreId,

            s.Name AS StoreName,

            SUM(sd.PeopleIn) AS Visitors,

            SUM(sd.PeopleOut) AS Exits

        FROM dbo.StoreDailyMetrics sd

        INNER JOIN dbo.Stores s
            ON s.Id = sd.StoreId

        WHERE sd.MetricDate >= CAST(@FromUtc AS DATE)
          AND sd.MetricDate <= CAST(@ToUtc AS DATE)

          AND s.IsDeleted = 0

        GROUP BY
            s.Id,
            s.Name

        ORDER BY
            SUM(sd.PeopleIn) DESC;
        """;

            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<StoreComparisonAnalyticsDto>(
                sql,
                new
                {
                    Top = top,
                    FromUtc = fromUtc,
                    ToUtc = toUtc
                });
        }

    public async Task<IEnumerable<DayComparisonAnalyticsDto>> GetDayComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var previousFrom = fromUtc.AddDays(-7);
        var previousTo = toUtc.AddDays(-7);

        const string sql = """
        WITH CurrentWeek AS
        (
            SELECT
                DATENAME(WEEKDAY, MetricDate) AS DayName,
                DATEPART(WEEKDAY, MetricDate) AS DayOrder,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY
                DATENAME(WEEKDAY, MetricDate),
                DATEPART(WEEKDAY, MetricDate)
        ),
        PreviousWeek AS
        (
            SELECT
                DATENAME(WEEKDAY, MetricDate) AS DayName,
                DATEPART(WEEKDAY, MetricDate) AS DayOrder,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreDailyMetrics
            WHERE MetricDate >= CAST(@PreviousFrom AS DATE)
              AND MetricDate <= CAST(@PreviousTo AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY
                DATENAME(WEEKDAY, MetricDate),
                DATEPART(WEEKDAY, MetricDate)
        )

        SELECT
            COALESCE(c.DayName,p.DayName) AS DayName,

            ISNULL(c.Visitors,0) AS CurrentVisitors,

            ISNULL(p.Visitors,0) AS PreviousVisitors

        FROM CurrentWeek c

        FULL OUTER JOIN PreviousWeek p
            ON c.DayName = p.DayName

        ORDER BY
            COALESCE(c.DayOrder,p.DayOrder);
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<DayComparisonAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                PreviousFrom = previousFrom,
                PreviousTo = previousTo
            });
    }
    public async Task<IEnumerable<HourComparisonAnalyticsDto>> GetHourComparisonAsync(
        Guid? storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var previousFrom = fromUtc.AddDays(-1);
        var previousTo = toUtc.AddDays(-1);

        const string sql = """
        WITH CurrentPeriod AS
        (
            SELECT
                MetricHour AS Hour,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreHourlyMetrics
            WHERE MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY MetricHour
        ),
        PreviousPeriod AS
        (
            SELECT
                MetricHour AS Hour,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreHourlyMetrics
            WHERE MetricDate >= CAST(@PreviousFrom AS DATE)
              AND MetricDate <= CAST(@PreviousTo AS DATE)
              AND (@StoreId IS NULL OR StoreId = @StoreId)
            GROUP BY MetricHour
        )

        SELECT

            COALESCE(c.Hour, p.Hour) AS Hour,

            ISNULL(c.Visitors,0) AS CurrentVisitors,

            ISNULL(p.Visitors,0) AS PreviousVisitors

        FROM CurrentPeriod c

        FULL OUTER JOIN PreviousPeriod p
            ON c.Hour = p.Hour

        ORDER BY Hour;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<HourComparisonAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                PreviousFrom = previousFrom,
                PreviousTo = previousTo
            });
    }

    public async Task<IEnumerable<StoreVsStoreHourDto>> GetStoreVsStoreHourAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        WITH PrimaryStore AS
        (
            SELECT
                MetricHour AS HourNumber,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreHourlyMetrics
            WHERE StoreId = @PrimaryStoreId
              AND MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
            GROUP BY MetricHour
        ),
        ComparisonStore AS
        (
            SELECT
                MetricHour AS HourNumber,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreHourlyMetrics
            WHERE StoreId = @ComparisonStoreId
              AND MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
            GROUP BY MetricHour
        )

        SELECT

            COALESCE(p.HourNumber,c.HourNumber) AS Hour,

            ISNULL(p.Visitors,0) AS PrimaryVisitors,

            ISNULL(c.Visitors,0) AS ComparisonVisitors

        FROM PrimaryStore p

        FULL OUTER JOIN ComparisonStore c
            ON p.HourNumber = c.HourNumber

        ORDER BY
            COALESCE(p.HourNumber,c.HourNumber);
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<StoreVsStoreHourDto>(
            sql,
            new
            {
                PrimaryStoreId = primaryStoreId,
                ComparisonStoreId = comparisonStoreId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<StoreVsStoreDayDto>> GetStoreVsStoreDayAsync(
        Guid primaryStoreId,
        Guid comparisonStoreId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        WITH PrimaryStore AS
        (
            SELECT
                MetricDate AS DayDate,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreDailyMetrics
            WHERE StoreId = @PrimaryStoreId
              AND MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
            GROUP BY MetricDate
        ),
        ComparisonStore AS
        (
            SELECT
                MetricDate AS DayDate,
                SUM(PeopleIn) AS Visitors
            FROM dbo.StoreDailyMetrics
            WHERE StoreId = @ComparisonStoreId
              AND MetricDate >= CAST(@FromUtc AS DATE)
              AND MetricDate <= CAST(@ToUtc AS DATE)
            GROUP BY MetricDate
        )

        SELECT

            DATENAME(
                WEEKDAY,
                COALESCE(p.DayDate,c.DayDate)
            ) AS DayName,

            ISNULL(p.Visitors,0) AS PrimaryVisitors,

            ISNULL(c.Visitors,0) AS ComparisonVisitors

        FROM PrimaryStore p

        FULL OUTER JOIN ComparisonStore c
            ON p.DayDate = c.DayDate

        ORDER BY
            COALESCE(p.DayDate,c.DayDate);
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<StoreVsStoreDayDto>(
            sql,
            new
            {
                PrimaryStoreId = primaryStoreId,
                ComparisonStoreId = comparisonStoreId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<MeasurementPointAnalyticsDto>> GetMeasurementPointAnalyticsAsync(
    Guid storeId,
    DateTime fromUtc,
    DateTime toUtc)
    {
        const string sql = """
        SELECT
            mp.Id AS MeasurementPointId,
            mp.Name AS MeasurementPointName,
            SUM(dr.PeopleIn) AS Visitors,
            SUM(dr.PeopleOut) AS Exits,
            SUM(dr.PeopleIn - dr.PeopleOut) AS NetTraffic,
            MAX(dr.Occupancy) AS PeakOccupancy,
            CAST(
                AVG(CAST(dr.Occupancy AS decimal(18,2)))
                AS decimal(18,2)
            ) AS AvgOccupancy
        FROM dbo.DeviceReadings dr
        INNER JOIN dbo.MeasurementPoints mp
            ON mp.Id = dr.MeasurementPointId
        WHERE dr.StoreId = @StoreId
          AND dr.MeasurementPointId IS NOT NULL
          AND dr.ReadingTimestampUtc >= @FromUtc
          AND dr.ReadingTimestampUtc <= @ToUtc
        GROUP BY
            mp.Id,
            mp.Name
        ORDER BY
            Visitors DESC;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<MeasurementPointAnalyticsDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<IEnumerable<TopMeasurementPointDto>>
        GetTopMeasurementPointsAsync(
            Guid storeId,
            DateTime fromUtc,
            DateTime toUtc,
            int top = 10)
    {
        const string sql = """
        SELECT TOP (@Top)

            mp.Id AS MeasurementPointId,

            mp.Name AS MeasurementPointName,

            SUM(dr.PeopleIn) AS Visitors

        FROM dbo.DeviceReadings dr

        INNER JOIN dbo.MeasurementPoints mp
            ON mp.Id = dr.MeasurementPointId

        WHERE dr.StoreId = @StoreId

          AND dr.MeasurementPointId IS NOT NULL

          AND dr.ReadingTimestampUtc >= @FromUtc

          AND dr.ReadingTimestampUtc <= @ToUtc

        GROUP BY
            mp.Id,
            mp.Name

        ORDER BY
            Visitors DESC;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        var rows =
            (await connection.QueryAsync<TopMeasurementPointDto>(
                sql,
                new
                {
                    StoreId = storeId,
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    Top = top
                }))
            .ToList();

        for (var i = 0; i < rows.Count; i++)
        {
            rows[i].Rank = i + 1;
        }

        return rows;
    }

    public async Task<IEnumerable<MeasurementPointDistributionDto>>
    GetMeasurementPointDistributionAsync(
        Guid storeId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        WITH Traffic AS
        (
            SELECT
                mp.Id AS MeasurementPointId,
                mp.Name AS MeasurementPointName,
                SUM(dr.PeopleIn) AS Visitors
            FROM dbo.DeviceReadings dr
            INNER JOIN dbo.MeasurementPoints mp
                ON mp.Id = dr.MeasurementPointId
            WHERE dr.StoreId = @StoreId
              AND dr.MeasurementPointId IS NOT NULL
              AND dr.ReadingTimestampUtc >= @FromUtc
              AND dr.ReadingTimestampUtc <= @ToUtc
            GROUP BY
                mp.Id,
                mp.Name
        )
        SELECT
            MeasurementPointId,
            MeasurementPointName,
            Visitors,
            CAST(
                Visitors * 100.0 /
                NULLIF(SUM(Visitors) OVER(),0)
                AS decimal(18,2)
            ) AS TrafficPercentage
        FROM Traffic
        ORDER BY Visitors DESC;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<
            MeasurementPointDistributionDto>(
            sql,
            new
            {
                StoreId = storeId,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    public async Task<MeasurementPointComparisonDto?>
    GetMeasurementPointComparisonAsync(
        Guid primaryMeasurementPointId,
        Guid comparisonMeasurementPointId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        const string sql = """
        WITH Metrics AS
        (
            SELECT
                mp.Id,
                mp.Name,
                SUM(dr.PeopleIn) AS Visitors,
                MAX(dr.Occupancy) AS PeakOccupancy
            FROM dbo.DeviceReadings dr
            INNER JOIN dbo.MeasurementPoints mp
                ON mp.Id = dr.MeasurementPointId
            WHERE dr.MeasurementPointId IN
            (
                @PrimaryMeasurementPointId,
                @ComparisonMeasurementPointId
            )
            AND dr.ReadingTimestampUtc >= @FromUtc
            AND dr.ReadingTimestampUtc <= @ToUtc
            GROUP BY
                mp.Id,
                mp.Name
        )
        SELECT
            p.Id AS PrimaryMeasurementPointId,
            p.Name AS PrimaryMeasurementPointName,

            c.Id AS ComparisonMeasurementPointId,
            c.Name AS ComparisonMeasurementPointName,

            p.Visitors AS PrimaryVisitors,
            c.Visitors AS ComparisonVisitors,

            p.PeakOccupancy AS PrimaryPeakOccupancy,
            c.PeakOccupancy AS ComparisonPeakOccupancy,

            p.Visitors - c.Visitors AS VisitorDifference,

            p.PeakOccupancy - c.PeakOccupancy
                AS OccupancyDifference

        FROM Metrics p
        CROSS JOIN Metrics c

        WHERE p.Id = @PrimaryMeasurementPointId
          AND c.Id = @ComparisonMeasurementPointId;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<
            MeasurementPointComparisonDto>(
            sql,
            new
            {
                PrimaryMeasurementPointId =
                    primaryMeasurementPointId,

                ComparisonMeasurementPointId =
                    comparisonMeasurementPointId,

                FromUtc = fromUtc,
                ToUtc = toUtc
            });
    }

    //MAPA

    public async Task<IReadOnlyList<StoreMapDto>>
    GetStoresMapAsync(
        Guid tenantId)
    {
        const string sql =
            """
            SELECT

                s.Id                 AS StoreId,
                s.Name               AS StoreName,

                s.Country,
                s.City,

                ISNULL(s.Latitude,0)  AS Latitude,
                ISNULL(s.Longitude,0) AS Longitude,

                (
                    SELECT COUNT(*)
                    FROM dbo.Alerts a
                    WHERE a.StoreId = s.Id
                    AND a.IsResolved = 0
                ) AS OpenAlerts,

                (
                    SELECT COUNT(*)
                    FROM dbo.Devices d
                    WHERE d.StoreId = s.Id
                    AND d.IsOnline = 1
                ) AS OnlineDevices

            FROM dbo.Stores s
            WHERE s.TenantId = @TenantId
            AND s.IsDeleted = 0
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var rows =
            await connection.QueryAsync<dynamic>(
                sql,
                new { TenantId = tenantId });

        return rows
            .Select(x => new StoreMapDto
            {
                StoreId = x.StoreId,
                StoreName = x.StoreName,

                Country = x.Country,
                City = x.City,

                Latitude = x.Latitude,
                Longitude = x.Longitude,

                CurrentOccupancy = 0,

                OpenAlerts = x.OpenAlerts,

                IsOnline = x.OnlineDevices > 0,

                Status =
                    x.OpenAlerts > 0
                        ? "Warning"
                        : "Healthy"
            })
            .ToList();
    }

    // FORECAST - PREDICCION POR ESTADISTICA -V1

    public async Task<ForecastDto> GetForecastAsync(
    Guid storeId,
    DateTime targetDateUtc)
    {
        const string sql =
            """
            DECLARE @TargetDate date = CAST(@TargetDateUtc AS date);

            WITH EquivalentDays AS
            (
                SELECT TOP 4
                    MetricDate
                FROM dbo.StoreDailyMetrics
                WHERE StoreId = @StoreId
                  AND MetricDate < @TargetDate
                  AND DATEPART(WEEKDAY, MetricDate) = DATEPART(WEEKDAY, @TargetDate)
                ORDER BY MetricDate DESC
            ),
            DailyBase AS
            (
                SELECT
                    COUNT(*) AS HistoricalDaysUsed,
                    AVG(CAST(PeopleIn AS decimal(18,2))) AS AvgPeopleIn,
                    AVG(CAST(PeopleOut AS decimal(18,2))) AS AvgPeopleOut,
                    AVG(CAST(PeakOccupancy AS decimal(18,2))) AS AvgPeakOccupancy
                FROM dbo.StoreDailyMetrics
                WHERE StoreId = @StoreId
                  AND MetricDate IN (SELECT MetricDate FROM EquivalentDays)
            ),
            HourlyBase AS
            (
                SELECT
                    MetricHour,
                    AVG(CAST(PeopleIn AS decimal(18,2))) AS AvgVisitors
                FROM dbo.StoreHourlyMetrics
                WHERE StoreId = @StoreId
                  AND MetricDate IN (SELECT MetricDate FROM EquivalentDays)
                GROUP BY MetricHour
            ),
            PeakHour AS
            (
                SELECT TOP 1
                    MetricHour,
                    AvgVisitors
                FROM HourlyBase
                ORDER BY AvgVisitors DESC
            )

            SELECT
                @StoreId AS StoreId,
                CAST(@TargetDate AS datetime2) AS TargetDateUtc,

                CAST(ISNULL(ROUND(db.AvgPeopleIn, 0), 0) AS int) AS PredictedVisitors,
                CAST(ISNULL(ROUND(db.AvgPeopleOut, 0), 0) AS int) AS PredictedPeopleOut,
                CAST(ISNULL(ROUND(db.AvgPeopleIn - db.AvgPeopleOut, 0), 0) AS int) AS PredictedNetTraffic,

                CAST(ISNULL(ph.MetricHour, 0) AS int) AS PredictedPeakHour,
                CAST(ISNULL(ROUND(ph.AvgVisitors, 0), 0) AS int) AS PredictedPeakHourVisitors,

                CAST(ISNULL(ROUND(db.AvgPeakOccupancy, 0), 0) AS int) AS PredictedPeakOccupancy,

                CAST(
                    CASE
                        WHEN ISNULL(db.HistoricalDaysUsed, 0) >= 4 THEN 90
                        WHEN ISNULL(db.HistoricalDaysUsed, 0) = 3 THEN 75
                        WHEN ISNULL(db.HistoricalDaysUsed, 0) = 2 THEN 60
                        WHEN ISNULL(db.HistoricalDaysUsed, 0) = 1 THEN 40
                        ELSE 0
                    END
                AS decimal(18,2)) AS Confidence,

                ISNULL(db.HistoricalDaysUsed, 0) AS HistoricalDaysUsed,

                'EquivalentWeekdayAverageV1' AS Method

            FROM DailyBase db
            OUTER APPLY
            (
                SELECT TOP 1
                    MetricHour,
                    AvgVisitors
                FROM PeakHour
            ) ph;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QuerySingleAsync<ForecastDto>(
                sql,
                new
                {
                    StoreId = storeId,
                    TargetDateUtc = targetDateUtc
                });

        return result;
    }

    public async Task<int> GetCurrentHourVisitorsAsync(
        Guid storeId,
        DateTime utcNow)
    {
        const string sql =
            """
        SELECT
            ISNULL(SUM(PeopleIn),0)
        FROM dbo.StoreHourlyMetrics
        WHERE StoreId = @StoreId
          AND MetricDate = CAST(@UtcNow AS date)
          AND MetricHour = DATEPART(HOUR,@UtcNow)
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            sql,
            new
            {
                StoreId = storeId,
                UtcNow = utcNow
            });
    }

}