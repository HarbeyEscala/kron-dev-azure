namespace Kron.Counting.Application.DTOs;

public sealed class DeviceAssignmentContext
{
    public Guid DeviceAssignmentId { get; set; }

    public Guid MeasurementPointId { get; set; }

    public Guid StoreId { get; set; }
}