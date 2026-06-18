using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IStoreRepository
{
    Task<IEnumerable<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Store?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(Store store, CancellationToken cancellationToken = default);

    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default);
}