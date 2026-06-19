using Kron.Counting.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class DailyMetricsMaterializerBackgroundService
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DailyMetricsMaterializerBackgroundService(
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
                        IDailyMetricsMaterializerService>();

                await service.MaterializeAsync(
                    stoppingToken);

                Console.WriteLine(
                    $"DAILY MATERIALIZER -> {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"DAILY MATERIALIZER ERROR -> {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromHours(4),
                stoppingToken);
        }   
    }
}