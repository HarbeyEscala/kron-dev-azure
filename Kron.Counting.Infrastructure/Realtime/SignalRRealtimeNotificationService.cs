using Kron.Counting.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Kron.Counting.Infrastructure.Realtime;

public sealed class SignalRRealtimeNotificationService
    : IRealtimeNotificationService
{
    private readonly IHubContext<AnalyticsHub> _hubContext;

    public SignalRRealtimeNotificationService(
        IHubContext<AnalyticsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task AnalyticsUpdatedAsync(
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"SIGNALR -> AnalyticsUpdated -> {DateTime.UtcNow}");

        await _hubContext.Clients.All.SendAsync(
            "AnalyticsUpdated",
            cancellationToken);
    }

    public async Task AlertCreatedAsync(
        Guid tenantId,
        object payload)
    {
        Console.WriteLine(
            $"SIGNALR -> AlertCreated -> {tenantId}");

        await _hubContext.Clients.All.SendAsync(
            "alert-created",
            payload);
    }

    public async Task AlertResolvedAsync(
        Guid tenantId,
        Guid alertId)
    {
        Console.WriteLine(
            $"SIGNALR -> AlertResolved -> {alertId}");

        await _hubContext.Clients.All.SendAsync(
            "alert-resolved",
            alertId);
    }
}