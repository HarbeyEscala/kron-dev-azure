namespace Kron.Counting.Application.Interfaces;

public interface IRealtimeNotificationService
{
    Task AnalyticsUpdatedAsync(
        CancellationToken cancellationToken = default);
}