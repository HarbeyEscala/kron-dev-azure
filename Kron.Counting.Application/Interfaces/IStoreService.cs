using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;

namespace Kron.Counting.Application.Interfaces;

public interface IStoreService
{
    Task<IEnumerable<StoreDto>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<StoreDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateStoreRequestDto request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateStoreRequestDto request,
        CancellationToken cancellationToken = default);

    Task PatchAsync(
        Guid id,
        PatchStoreRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}