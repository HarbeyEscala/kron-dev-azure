using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class TelemetryService : ITelemetryService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceReadingRepository _deviceReadingRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public TelemetryService(
        IDeviceRepository deviceRepository,
        IDeviceReadingRepository deviceReadingRepository,
        IDashboardRepository dashboardRepository)
    {
        _deviceRepository = deviceRepository;
        _deviceReadingRepository = deviceReadingRepository;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<long> IngestReadingAsync(
        Guid deviceId,
        IngestCounterSnapshotRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                deviceId,
                cancellationToken);

        if (device is null)
            throw new KeyNotFoundException("Device not found.");

        if (!device.IsActive || device.IsDeleted)
            throw new InvalidOperationException("Device is inactive.");

        if (!device.StoreId.HasValue)
        {
            return 0;
        }
        /*
         * TEMPORAL MVP LOGIC
         *
         * CHP015 real probablemente envía contadores acumulados.
         *
         * Más adelante:
         *
         * deltaIn  = request.TotalIn  - device.LastTotalIn
         * deltaOut = request.TotalOut - device.LastTotalOut
         *
         * Por ahora:
         * usamos valores directos para mantener pipeline vivo.
         */

        var peopleIn =
            request.TotalIn - device.LastTotalIn;

        var peopleOut =
            request.TotalOut - device.LastTotalOut;

        if (peopleIn < 0)
            peopleIn = 0;

        if (peopleOut < 0)
            peopleOut = 0;

        var reading = new DeviceReading
        {
            DeviceId = deviceId,
            ReadingTimestampUtc = request.ReadingTimestampUtc,
            PeopleIn = peopleIn,
            PeopleOut = peopleOut,
            Occupancy = Math.Max(0, peopleIn - peopleOut),
            RawPayloadJson =
                System.Text.Json.JsonSerializer.Serialize(request),
            CreatedAtUtc = DateTime.UtcNow
        };

        var readingId =
            await _deviceReadingRepository.CreateAsync(
                reading,
                cancellationToken);

        await _deviceRepository.UpdateHeartbeatAsync(
            deviceId,
            DateTime.UtcNow,
            true,
            cancellationToken);

        var snapshot =
            await _dashboardRepository.GetSnapshotByStoreIdAsync(
                device.StoreId.Value,
                cancellationToken);

        var occupancy =
            Math.Max(0, peopleIn - peopleOut);

        if (snapshot is null)
        {
            snapshot = new LiveDashboardSnapshot
            {
                Id = Guid.NewGuid(),
                StoreId = device.StoreId.Value,
                CurrentOccupancy = occupancy,
                TodayIn = peopleIn,
                TodayOut = peopleOut,
                LastReadingAtUtc = request.ReadingTimestampUtc,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }
        else
        {
            snapshot.CurrentOccupancy = occupancy;
            snapshot.TodayIn += peopleIn;
            snapshot.TodayOut += peopleOut;
            snapshot.LastReadingAtUtc = request.ReadingTimestampUtc;
            snapshot.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dashboardRepository.UpsertSnapshotAsync(
            snapshot,
            cancellationToken);

        device.LastTotalIn = request.TotalIn;
        device.LastTotalOut = request.TotalOut;
        device.UpdatedAtUtc = DateTime.UtcNow;

        await _deviceRepository.UpdateAsync(
            device,
            cancellationToken);

        return readingId;
    }
}