using Kron.Counting.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class MonthlyMetricsMaterializerBackgroundService
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MonthlyMetricsMaterializerBackgroundService(
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
                        IMonthlyMetricsMaterializerService>();

                await service.MaterializeAsync(
                    stoppingToken);

                Console.WriteLine(
                    $"MONTHLY MATERIALIZER -> {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MONTHLY MATERIALIZER ERROR -> {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromHours(12),
                stoppingToken);
        }
    }
}