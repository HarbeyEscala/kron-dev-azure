namespace Kron.Counting.Application.Interfaces;

public interface IHourlyMetricsMaterializerService
{
    Task MaterializeAsync(
        CancellationToken cancellationToken = default);
}