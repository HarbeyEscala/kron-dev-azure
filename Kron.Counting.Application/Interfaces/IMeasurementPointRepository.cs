using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IMeasurementPointRepository
{
    Task<Guid> CreateAsync(MeasurementPoint measurementPoint);

    Task<MeasurementPoint?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<MeasurementPoint>> GetByStoreAsync(Guid storeId);
}