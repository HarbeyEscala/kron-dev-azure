using Microsoft.AspNetCore.SignalR;

namespace Kron.Counting.Infrastructure.Realtime;

public sealed class AlertsHub : Hub
{
    public async Task JoinTenant(
        Guid tenantId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            tenantId.ToString());
    }

    public async Task LeaveTenant(
        Guid tenantId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            tenantId.ToString());
    }
}