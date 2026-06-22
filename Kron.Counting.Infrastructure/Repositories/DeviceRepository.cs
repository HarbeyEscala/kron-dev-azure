using Dapper;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Infrastructure.Repositories;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeviceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Device>> GetByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                ProvisioningStatus,
                IpAddress,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE StoreId = @StoreId
              AND IsDeleted = 0
            ORDER BY Name;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Device>(
            sql,
            new { StoreId = storeId });
    }

    public async Task<Device?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE Id = @Id
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Device>(
            sql,
            new { Id = id });
    }

    public async Task<Device?> GetByApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE ApiKey = @ApiKey
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Device>(
            sql,
            new { ApiKey = apiKey });
    }

    public async Task<Device?> GetByIpAddressAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE IpAddress = @IpAddress
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Device>(
            sql,
            new
            {
                IpAddress = ipAddress
            });
    }

    public async Task<Device?> GetBySerialNumberAsync(
        string serialNumber,
    CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE SerialNumber = @SerialNumber
              AND IsDeleted = 0;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Device>(
            sql,
            new
            {
                SerialNumber = serialNumber
            });
    }
    public async Task<Device?> GetBySerialNumberAsync(
        Guid storeId,
        string serialNumber,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE StoreId = @StoreId
              AND SerialNumber = @SerialNumber
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Device>(
            sql,
            new
            {
                StoreId = storeId,
                SerialNumber = serialNumber
            });
    }

    public async Task<Guid> CreateAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO dbo.Devices
            (
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                IpAddress,
                ProvisioningStatus,
                LastTotalIn,
                LastTotalOut,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @TenantId,
                @StoreId,
                @SerialNumber,
                @Name,
                @DeviceType,
                @ApiKey,
                @IpAddress,
                @ProvisioningStatus,
                @LastTotalIn,
                @LastTotalOut,
                @FirmwareVersion,
                @LastSeenAtUtc,
                @LastPayloadUtc,
                @IsOnline,
                @IsActive,
                @IsDeleted,
                @CreatedAtUtc
            );
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, device);

        return device.Id;
    }

    public async Task UpdateAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Devices
            SET
                Name = @Name,
                DeviceType = @DeviceType,
                FirmwareVersion = @FirmwareVersion,
                ProvisioningStatus = @ProvisioningStatus,
                LastTotalIn = @LastTotalIn,
                LastTotalOut = @LastTotalOut,
                IpAddress = @IpAddress,
                IsActive = @IsActive,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, device);
    }

    public async Task UpdateApiKeyAsync(
        Guid deviceId,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Devices
            SET
                ApiKey = @ApiKey,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @DeviceId
              AND IsDeleted = 0;
        """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    DeviceId = deviceId,
                    ApiKey = apiKey,
                    UpdatedAtUtc = DateTime.UtcNow
                },
                cancellationToken: cancellationToken));
    }

    public async Task UpdateHeartbeatAsync(
        Guid id,
        DateTime lastSeenAtUtc,
        bool isOnline,
        CancellationToken cancellationToken = default)
    {
        await UpdateConnectionAsync(
            id,
            lastSeenAtUtc,
            isOnline,
            ipAddress: null,
            cancellationToken);
    }

    public async Task UpdateConnectionAsync(
        Guid id,
        DateTime lastSeenAtUtc,
        bool isOnline,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        MERGE dbo.DeviceTelemetryStatus AS target
        USING
        (
            SELECT @DeviceId AS DeviceId
        ) AS source
        ON target.DeviceId = source.DeviceId

        WHEN MATCHED THEN
            UPDATE SET
                LastSeenAtUtc = @LastSeenAtUtc,
                IsOnline = @IsOnline,
                IpAddress = COALESCE(@IpAddress, target.IpAddress),
                UpdatedAtUtc = @UpdatedAtUtc

        WHEN NOT MATCHED THEN
            INSERT
            (
                DeviceId,
                LastSeenAtUtc,
                LastPayloadUtc,
                IpAddress,
                IsOnline,
                UpdatedAtUtc
            )
            VALUES
            (
                @DeviceId,
                @LastSeenAtUtc,
                NULL,
                @IpAddress,
                @IsOnline,
                @UpdatedAtUtc
            );
    """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                DeviceId = id,
                LastSeenAtUtc = lastSeenAtUtc,
                IsOnline = isOnline,
                IpAddress = ipAddress,
                UpdatedAtUtc = DateTime.UtcNow
            });
    }

    public async Task UpdateTelemetryStatusAsync(
        Guid deviceId,
        DateTime lastSeenAtUtc,
        DateTime lastPayloadUtc,
        bool isOnline,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        MERGE dbo.DeviceTelemetryStatus AS target
        USING
        (
            SELECT
                @DeviceId AS DeviceId
        ) AS source
        ON target.DeviceId = source.DeviceId

        WHEN MATCHED THEN
            UPDATE SET
                LastSeenAtUtc = @LastSeenAtUtc,
                LastPayloadUtc = @LastPayloadUtc,
                IsOnline = @IsOnline,
                IpAddress = COALESCE(@IpAddress, target.IpAddress),
                UpdatedAtUtc = @UpdatedAtUtc

        WHEN NOT MATCHED THEN
            INSERT
            (
                DeviceId,
                LastSeenAtUtc,
                LastPayloadUtc,
                IpAddress,
                IsOnline,
                UpdatedAtUtc
            )
            VALUES
            (
                @DeviceId,
                @LastSeenAtUtc,
                @LastPayloadUtc,
                @IpAddress,
                @IsOnline,
                @UpdatedAtUtc
            );
    """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                DeviceId = deviceId,
                LastSeenAtUtc = lastSeenAtUtc,
                LastPayloadUtc = lastPayloadUtc,
                IsOnline = isOnline,
                IpAddress = ipAddress,
                UpdatedAtUtc = DateTime.UtcNow
            });
    }

    public async Task UpdateLastPayloadAsync(
        Guid deviceId,
        DateTime payloadUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
        UPDATE dbo.Devices
        SET
            LastPayloadUtc = @PayloadUtc,
            UpdatedAtUtc = @UpdatedAtUtc
        WHERE Id = @DeviceId
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                DeviceId = deviceId,
                PayloadUtc = payloadUtc,
                UpdatedAtUtc = DateTime.UtcNow
            });
    }
    public async Task<IEnumerable<Device>> GetPendingAsync(
    CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            Id,
            TenantId,
            StoreId,
            SerialNumber,
            Name,
            DeviceType,
            ApiKey,
            IpAddress,
            ProvisioningStatus,
            FirmwareVersion,
            LastSeenAtUtc,
            LastPayloadUtc,
            IsOnline,
            IsActive,
            IsDeleted,
            CreatedAtUtc,
            UpdatedAtUtc,
            DeletedAtUtc
        FROM dbo.Devices
        WHERE ProvisioningStatus = 'Pending'
          AND IsDeleted = 0
        ORDER BY CreatedAtUtc DESC;
    """;

        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Device>(sql);
    }

    public async Task ProvisionAsync(
        Guid deviceId,
        Guid tenantId,
        Guid storeId,
        string name,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        UPDATE dbo.Devices
        SET
            TenantId = @TenantId,
            StoreId = @StoreId,
            Name = @Name,
            ProvisioningStatus = 'Provisioned',
            UpdatedAtUtc = @UpdatedAtUtc
        WHERE Id = @DeviceId
          AND IsDeleted = 0;
    """;

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            sql,
            new
            {
                DeviceId = deviceId,
                TenantId = tenantId,
                StoreId = storeId,
                Name = name,
                UpdatedAtUtc = DateTime.UtcNow
            });
    }

    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Devices
            SET
                IsDeleted = 1,
                IsActive = 0,
                IsOnline = 0,
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

    public async Task<IEnumerable<Device>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                TenantId,
                StoreId,
                SerialNumber,
                Name,
                DeviceType,
                ApiKey,
                ProvisioningStatus,
                IpAddress,
                FirmwareVersion,
                LastSeenAtUtc,
                LastPayloadUtc,
                IsOnline,
                IsActive,
                IsDeleted,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeletedAtUtc
            FROM dbo.Devices
            WHERE IsDeleted = 0;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Device>(
            sql);
    }

    public async Task<IReadOnlyList<DeviceAlertSnapshot>>
        GetAlertSnapshotsAsync()
    {
        const string sql =
            """
        SELECT
            d.Id              AS DeviceId,
            d.TenantId,
            d.Name,
            d.IsActive,
            d.IsDeleted,

            ISNULL(ts.IsOnline, 0)
                AS IsOnline,

            ts.LastSeenAtUtc,
            ts.LastPayloadUtc

        FROM dbo.Devices d

        LEFT JOIN dbo.DeviceTelemetryStatus ts
            ON ts.DeviceId = d.Id

        WHERE d.IsDeleted = 0
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        var result =
            await connection.QueryAsync<DeviceAlertSnapshot>(
                sql);

        return result.ToList();
    }
}