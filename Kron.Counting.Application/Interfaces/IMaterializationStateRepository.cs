using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces.Repositories;

public interface IMaterializationStateRepository
{
    Task<MaterializationState?> GetByProcessAsync(string processName);

    Task<long> CreateAsync(MaterializationState state);

    Task UpdateAsync(MaterializationState state);
}