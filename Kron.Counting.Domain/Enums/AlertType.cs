namespace Kron.Counting.Domain.Enums;

public static class AlertType
{
    public const string DeviceOffline =
        "DeviceOffline";

    public const string DeviceSilent =
        "DeviceSilent";

    public const string PayloadFailure =
        "PayloadFailure";

    public const string FirmwareOutdated =
        "FirmwareOutdated";

    public const string TrafficDrop =
        "TrafficDrop";

    public const string TrafficSpike =
        "TrafficSpike";
}