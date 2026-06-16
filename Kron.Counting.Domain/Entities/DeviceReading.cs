namespace Kron.Counting.Domain.Entities;

public sealed class DeviceReading
{
    public long Id { get; set; }

    public Guid DeviceId { get; set; }

    public DateTime ReadingTimestampUtc { get; set; }

    public int PeopleIn { get; set; }

    public int PeopleOut { get; set; }

    public int Occupancy { get; set; }

    public Guid? DeviceAssignmentId { get; set; }

    public Guid? MeasurementPointId { get; set; }

    public Guid? StoreId { get; set; }

    public string? RawPayloadJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Device? Device { get; set; }
}