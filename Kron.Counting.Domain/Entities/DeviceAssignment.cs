namespace Kron.Counting.Domain.Entities;

public sealed class DeviceAssignment
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Guid MeasurementPointId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public DateTime? UnassignedAtUtc { get; set; }

    public int BaselineTotalIn { get; set; }

    public int BaselineTotalOut { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Device? Device { get; set; }

    public MeasurementPoint? MeasurementPoint { get; set; }
}