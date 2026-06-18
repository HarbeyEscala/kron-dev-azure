using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Constants;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Domain.Enums;

namespace Kron.Counting.Application.Services;

public sealed class TrafficAnomalyDetectorService
{
    private const double DropThreshold = 0.60;
    private const double SpikeThreshold = 1.50;

    private readonly IStoreRepository _storeRepository;
    private readonly IAnalyticsService _analyticsService;
    private readonly IAlertRepository _alertRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public TrafficAnomalyDetectorService(
        IStoreRepository storeRepository,
        IAnalyticsService analyticsService,
        IAlertRepository alertRepository,
        IRealtimeNotificationService realtimeNotificationService)
    {
        _storeRepository = storeRepository;
        _analyticsService = analyticsService;
        _alertRepository = alertRepository;
        _realtimeNotificationService = realtimeNotificationService;
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine(
            "TRAFFIC ANOMALY DETECTOR START");
        var utcNow = DateTime.UtcNow;

        var stores =
            await _storeRepository.GetAllAsync();
        Console.WriteLine(
            $"STORES FOUND -> {stores.Count}");
        foreach (var store in stores)
        {
            if (!store.IsActive || store.IsDeleted)
                continue;

            var forecast =
                await _analyticsService.GetForecastAsync(
                    store.Id,
                    utcNow.Date);

            Console.WriteLine(
                $"FORECAST -> {store.Name} | Expected={forecast.PredictedPeakHourVisitors}");

            if (forecast.PredictedPeakHourVisitors <= 0)
                continue;

            var expected =
                forecast.PredictedPeakHourVisitors;

            var actual =
                await _analyticsService
                    .GetCurrentHourVisitorsAsync(
                        store.Id);

            Console.WriteLine($"ANOMALY -> {store.Name} | Expected={expected} | Actual={actual}");

            if (actual <= 0)
                continue;

            var ratio =
                (double)actual / expected;

            Console.WriteLine(
                $"RATIO -> {store.Name} | Ratio={ratio}");

            await EvaluateDropAsync(
                store,
                ratio,
                actual,
                expected,
                utcNow);

            await EvaluateSpikeAsync(
                store,
                ratio,
                actual,
                expected,
                utcNow);
        }
    }

    private async Task EvaluateDropAsync(
        Domain.Entities.Store store,
        double ratio,
        int actual,
        int expected,
        DateTime utcNow)
    {
        var openAlert =
            await _alertRepository
                .GetOpenStoreAlertAsync(
                    store.TenantId,
                    store.Id,
                    AlertType.TrafficDrop);

        if (ratio > DropThreshold)
        {
            if (openAlert is not null)
            {
                await _alertRepository.ResolveAsync(
                    openAlert.Id);

                await _realtimeNotificationService
                    .AlertResolvedAsync(
                        store.TenantId,
                        openAlert.Id);
            }

            return;
        }

        if (openAlert is not null)
        {
            await _alertRepository.TouchAsync(
                openAlert.Id);

            return;
        }

        var alert =
            new Alert
            {
                Id = Guid.NewGuid(),
                TenantId = store.TenantId,
                StoreId = store.Id,
                Source = AlertSource.Analytics,
                Type = AlertType.TrafficDrop,
                Severity = AlertSeverity.Warning,
                Message =
                    $"Store '{store.Name}' traffic is below forecast. Expected {expected}, actual {actual}.",
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

    private async Task EvaluateSpikeAsync(
        Domain.Entities.Store store,
        double ratio,
        int actual,
        int expected,
        DateTime utcNow)
    {
        var openAlert =
            await _alertRepository
                .GetOpenStoreAlertAsync(
                    store.TenantId,
                    store.Id,
                    AlertType.TrafficSpike);

        if (ratio < SpikeThreshold)
        {
            if (openAlert is not null)
            {
                await _alertRepository.ResolveAsync(
                    openAlert.Id);

                await _realtimeNotificationService
                    .AlertResolvedAsync(
                        store.TenantId,
                        openAlert.Id);
            }

            return;
        }

        if (openAlert is not null)
        {
            await _alertRepository.TouchAsync(
                openAlert.Id);

            return;
        }

        var alert =
            new Alert
            {
                Id = Guid.NewGuid(),
                TenantId = store.TenantId,
                StoreId = store.Id,
                Source = AlertSource.Analytics,
                Type = AlertType.TrafficSpike,
                Severity = AlertSeverity.Warning,
                Message =
                    $"Store '{store.Name}' traffic is above forecast. Expected {expected}, actual {actual}.",
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