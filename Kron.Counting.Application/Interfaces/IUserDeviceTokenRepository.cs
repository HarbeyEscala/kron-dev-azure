using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IUserDeviceTokenRepository
{
    Task RegisterAsync(
        UserDeviceToken token);

    Task<IReadOnlyList<UserDeviceToken>>
        GetActiveByTenantAsync(
            Guid tenantId);

    Task<IReadOnlyList<UserDeviceToken>>
        GetActiveByUserAsync(
            Guid userId);

    Task DeactivateAsync(
        string token);

    Task TouchAsync(
        string token);
}