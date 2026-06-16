namespace Kron.Counting.Application.Interfaces;

public interface IRealtimeNotificationService
{
    Task AnalyticsUpdatedAsync(
        CancellationToken cancellationToken = default);

    Task AlertCreatedAsync(
        Guid tenantId,
        object payload);

    Task AlertResolvedAsync(
        Guid tenantId,
        Guid alertId);
}