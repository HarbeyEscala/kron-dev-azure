namespace Kron.Counting.Application.DTOs;

public sealed class DeviceAssignmentLookup
{
    public Guid DeviceAssignmentId { get; set; }

    public Guid MeasurementPointId { get; set; }

    public Guid StoreId { get; set; }
}