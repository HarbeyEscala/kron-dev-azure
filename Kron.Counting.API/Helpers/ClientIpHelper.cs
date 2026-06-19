using System.Net;

namespace Kron.Counting.API.Helpers;

public static class ClientIpHelper
{
    private static readonly string[] IpHeaders =
    [
        "X-Client-IP",
        "X-Forwarded-For",
        "CLIENT-IP"
    ];

    public static string? Resolve(HttpContext httpContext)
    {
        foreach (var headerName in IpHeaders)
        {
            if (!httpContext.Request.Headers.TryGetValue(headerName, out var headerValue))
                continue;

            var candidate = headerValue.ToString().Split(',')[0].Trim();
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var normalized = Normalize(candidate);
            if (!string.IsNullOrWhiteSpace(normalized))
                return normalized;
        }

        return Normalize(httpContext.Connection.RemoteIpAddress);
    }

    public static string? Normalize(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return null;

        var value = ipAddress.Trim();

        var colonIndex = value.LastIndexOf(':');
        if (colonIndex > 0 && value.Contains('.'))
            value = value[..colonIndex];

        if (IPAddress.TryParse(value, out var parsed))
            return Normalize(parsed);

        return value;
    }

    private static string? Normalize(IPAddress? ipAddress)
    {
        if (ipAddress is null)
            return null;

        if (ipAddress.IsIPv4MappedToIPv6)
            ipAddress = ipAddress.MapToIPv4();

        return ipAddress.ToString();
    }
}
