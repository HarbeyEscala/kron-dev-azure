using Kron.Counting.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class OfflineAlertDetectorBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromHours(4);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OfflineAlertDetectorBackgroundService> _logger;

    public OfflineAlertDetectorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OfflineAlertDetectorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer =
            new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var detector =
                    scope.ServiceProvider
                        .GetRequiredService<OfflineAlertDetectorService>();

                await detector.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing offline alert detector.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}