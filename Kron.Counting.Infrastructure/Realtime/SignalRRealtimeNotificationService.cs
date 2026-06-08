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
}