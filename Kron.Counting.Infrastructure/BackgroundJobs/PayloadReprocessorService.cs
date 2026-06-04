using Kron.Counting.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Kron.Counting.Domain.Constants;

namespace Kron.Counting.Infrastructure.BackgroundJobs;

public sealed class PayloadReprocessorService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PayloadReprocessorService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IDevicePayloadRepository>();

                var failedPayloads =
                    await repository.GetFailedAsync(
                        100,
                        stoppingToken);

                foreach (var payload in failedPayloads)
                {
                    await repository.IncrementRetryAsync(
                        payload.Id,
                        stoppingToken);
                }

                const int MaxRetries = 5;

                foreach (var payload in failedPayloads)
                {
                    if (payload.RetryCount >= MaxRetries)
                    {
                        await repository.UpdateStatusAsync(
                            payload.Id,
                            PayloadStatuses.DeadLetter,
                            $"Maximum retry count exceeded ({payload.RetryCount})");

                        continue;
                    }

                    await repository.IncrementRetryAsync(
                        payload.Id,
                        stoppingToken);

                    Console.WriteLine(
                        $"REPROCESSOR -> RETRY {payload.Id} ({payload.RetryCount + 1})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"REPROCESSOR ERROR -> {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}