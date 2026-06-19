namespace Kron.Counting.Shared.Settings;

public sealed class TelemetrySettings
{
    public bool RequireApiKey { get; set; }

    public bool AllowLegacyIpIdentification { get; set; }

    public bool AllowAutoDiscovery { get; set; }

    public bool AllowLegacyWithoutApiKey { get; set; }
}
