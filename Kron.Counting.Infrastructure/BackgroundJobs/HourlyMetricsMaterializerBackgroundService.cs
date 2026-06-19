using Kron.Counting.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class HourlyMetricsMaterializerBackgroundService
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public HourlyMetricsMaterializerBackgroundService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _serviceProvider.CreateScope();

                var service =
                    scope.ServiceProvider.GetRequiredService<
                        IHourlyMetricsMaterializerService>();

                await service.MaterializeAsync(
                    stoppingToken);

                Console.WriteLine(
                    $"HOURLY MATERIALIZER -> {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"HOURLY MATERIALIZER ERROR -> {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromHours(1),
                stoppingToken);
        }
    }
}