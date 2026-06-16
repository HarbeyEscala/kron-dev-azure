namespace Kron.Counting.Application.DTOs;

public sealed class CreateDeviceAssignmentRequest
{
    public Guid DeviceId { get; set; }

    public Guid MeasurementPointId { get; set; }
}