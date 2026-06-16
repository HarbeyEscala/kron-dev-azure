namespace Kron.Counting.Application.DTOs.Responses;

public sealed class DeviceHealthSummaryDto
{
    public int TotalDevices { get; set; }

    public int OnlineDevices { get; set; }

    public int OfflineDevices { get; set; }

    public int ActiveDevices { get; set; }

    public int InactiveDevices { get; set; }
}