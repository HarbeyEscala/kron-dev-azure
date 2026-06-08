namespace Kron.Counting.Application.Interfaces;

public interface IDailyMetricsMaterializerService
{
    Task MaterializeAsync(
        CancellationToken cancellationToken = default);
}