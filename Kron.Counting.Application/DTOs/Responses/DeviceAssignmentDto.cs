namespace Kron.Counting.Application.DTOs.Responses;

public sealed class DeviceAssignmentDto
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Guid MeasurementPointId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public DateTime? UnassignedAtUtc { get; set; }

    public int BaselineTotalIn { get; set; }

    public int BaselineTotalOut { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
