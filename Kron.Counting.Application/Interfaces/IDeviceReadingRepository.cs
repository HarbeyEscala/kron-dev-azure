using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Interfaces;

public interface IDeviceReadingRepository
{
    Task<long> CreateAsync(DeviceReading reading, CancellationToken cancellationToken = default);

    Task<IEnumerable<DeviceReading>> GetByDeviceIdAsync(
        Guid deviceId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(
        Guid deviceId,
        DateTime readingTimestampUtc,
        int peopleIn,
        int peopleOut);
}