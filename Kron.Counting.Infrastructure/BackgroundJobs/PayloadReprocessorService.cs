using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IDevicePayloadProcessor>();

                var failedPayloads =
                    await repository.GetFailedAsync(
                        100,
                        stoppingToken);

                const int MaxRetries = 5;

                foreach (var payload in failedPayloads)
                {
                    if (payload.RetryCount >= MaxRetries)
                    {
                        await repository.UpdateStatusAsync(
                            payload.Id,
                            PayloadStatuses.DeadLetter,
                            $"Maximum retry count exceeded ({payload.RetryCount})");

                        Console.WriteLine(
                            $"REPROCESSOR -> DEAD LETTER {payload.Id}");

                        continue;
                    }

                    try
                    {
                        await repository.IncrementRetryAsync(
                            payload.Id,
                            stoppingToken);

                        await processor.ProcessAsync(
                            payload,
                            stoppingToken);

                        Console.WriteLine(
                            $"REPROCESSOR -> RECOVERED {payload.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"REPROCESSOR -> FAILED {payload.Id}");

                        Console.WriteLine(ex.Message);
                    }
                }

                Console.WriteLine();
                Console.WriteLine(
                    $"REPROCESSOR -> Failed Count = {failedPayloads.Count()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"REPROCESSOR ERROR -> {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(15),
                stoppingToken);
        }
    }
}