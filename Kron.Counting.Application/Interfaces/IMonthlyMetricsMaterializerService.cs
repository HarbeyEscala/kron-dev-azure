namespace Kron.Counting.Application.Interfaces;

public interface IMonthlyMetricsMaterializerService
{
    Task MaterializeAsync(
        CancellationToken cancellationToken = default);
}