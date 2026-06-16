using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Domain.Enums;

namespace Kron.Counting.Application.Services;

public sealed class SilentAlertDetectorService
{
    private const int SilentThresholdMinutes = 60;
    private const int CriticalThresholdMinutes = 180;

    private readonly IDeviceRepository _deviceRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IDeviceAssignmentResolver _deviceAssignmentResolver;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public SilentAlertDetectorService(
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
                x.IsOnline &&
                x.TenantId.HasValue)
            .ToList();

        foreach (var device in devices)
        {
            var openAlert =
                await _alertRepository.GetOpenAlertAsync(
                    device.TenantId!.Value,
                    device.Id,
                    AlertType.DeviceSilent);

            if (!device.LastPayloadUtc.HasValue)
            {
                continue;
            }

            var minutesSilent =
                (int)(utcNow - device.LastPayloadUtc.Value)
                    .TotalMinutes;

            if (minutesSilent < SilentThresholdMinutes)
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
                minutesSilent >= CriticalThresholdMinutes
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
                Type = AlertType.DeviceSilent,
                Severity = severity,
                Message =
                    $"Device '{device.Name}' has not sent payloads for {minutesSilent} minutes.",
                CreatedAtUtc = utcNow,
                LastTriggeredAtUtc = utcNow,
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