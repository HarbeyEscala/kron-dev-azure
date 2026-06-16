using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Domain.Enums;

namespace Kron.Counting.Application.Services;

public sealed class OfflineAlertDetectorService
{
    private const int OfflineThresholdMinutes = 30;
    private const int CriticalThresholdMinutes = 120;

    private readonly IDeviceRepository _deviceRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IDeviceAssignmentResolver _deviceAssignmentResolver;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public OfflineAlertDetectorService(
        IDeviceRepository deviceRepository,
        IAlertRepository alertRepository,
        IDeviceAssignmentResolver deviceAssignmentResolver,
        IRealtimeNotificationService realtimeNotificationService)
    {
        _deviceRepository = deviceRepository;
        _alertRepository = alertRepository;
        _deviceAssignmentResolver = deviceAssignmentResolver;
        _realtimeNotificationService = realtimeNotificationService;
    }

    public async Task ExecuteAsync()
    {
        var utcNow = DateTime.UtcNow;

        var devices =
            (await _deviceRepository.GetAllAsync())
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.TenantId.HasValue)
            .ToList();

        foreach (var device in devices)
        {
            var openAlert =
                 await _alertRepository.GetOpenAlertAsync(
                     device.TenantId!.Value,
                     device.Id,
                     AlertType.DeviceOffline);

                if (device.IsOnline)
                {
                    if (openAlert is not null)
                    {
                        await _alertRepository.ResolveAsync(
                            openAlert.Id);

                        await _realtimeNotificationService
                            .AlertResolvedAsync(
                                device.TenantId.Value,
                                openAlert.Id);
                }

                    continue;
                }

                if (!device.LastSeenAtUtc.HasValue)
                    continue;

                var minutesOffline =
                    (int)(utcNow - device.LastSeenAtUtc.Value)
                        .TotalMinutes;

                if (minutesOffline < OfflineThresholdMinutes)
                    continue;

                if (openAlert is not null)
                {
                    await _alertRepository.TouchAsync(
                        openAlert.Id);

                    continue;
                }

            var context =
                await _deviceAssignmentResolver.ResolveAsync(
                    device.Id,
                    utcNow);

            var severity =
                minutesOffline >= CriticalThresholdMinutes
                    ? AlertSeverity.Critical
                    : AlertSeverity.Warning;

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                TenantId = device.TenantId.Value,
                DeviceId = device.Id,
                StoreId = context?.StoreId,
                MeasurementPointId = context?.MeasurementPointId,
                Source = AlertSource.Device,
                Type = AlertType.DeviceOffline,
                Severity = severity,
                Message =
                    $"Device '{device.Name}' has been offline for {minutesOffline} minutes.",
                CreatedAtUtc = utcNow,
                LastTriggeredAtUtc = utcNow,
                ResolvedAtUtc = null,
                IsResolved = false
            };

            await _alertRepository.CreateAsync(alert);

            await _realtimeNotificationService
                .AlertCreatedAsync(
                    alert.TenantId,
                    alert);
        }
    }
}