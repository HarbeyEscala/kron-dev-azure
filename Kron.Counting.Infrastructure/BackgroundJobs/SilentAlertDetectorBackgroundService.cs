using Kron.Counting.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class SilentAlertDetectorBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SilentAlertDetectorBackgroundService> _logger;

    public SilentAlertDetectorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SilentAlertDetectorBackgroundService> logger)
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
                        .GetRequiredService<SilentAlertDetectorService>();

                await detector.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing silent alert detector.");
            }

            await timer.WaitForNextTickAsync(
                stoppingToken);
        }
    }
}