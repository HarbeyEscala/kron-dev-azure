using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/internal/device-readings")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepository;

    private const string DemoStoreId =
        "0341DF21-4D90-4E3D-B054-BCD9EC47573D";

    public TelemetryController(
        IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    public async Task<IActionResult> Receive(
        CancellationToken cancellationToken)
    {
        Request.EnableBuffering();

        var now = DateTime.UtcNow;

        var ip =
            HttpContext.Connection
                .RemoteIpAddress?
                .ToString();

        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("================================");
        sb.AppendLine("HP015 REQUEST DETECTED");
        sb.AppendLine("================================");
        sb.AppendLine($"Timestamp UTC : {now:O}");
        sb.AppendLine($"Remote IP     : {ip}");
        sb.AppendLine($"Method        : {Request.Method}");
        sb.AppendLine($"Path          : {Request.Path}");
        sb.AppendLine($"QueryString   : {Request.QueryString}");
        sb.AppendLine($"ContentType   : {Request.ContentType}");
        sb.AppendLine();

        sb.AppendLine("HEADERS");
        sb.AppendLine("--------------------------------");

        foreach (var header in Request.Headers)
        {
            sb.AppendLine(
                $"{header.Key} = {header.Value}");
        }

        sb.AppendLine();

        if (Request.Query.Any())
        {
            sb.AppendLine("QUERY VALUES");
            sb.AppendLine("--------------------------------");

            foreach (var item in Request.Query)
            {
                sb.AppendLine(
                    $"{item.Key} = {item.Value}");
            }

            sb.AppendLine();
        }

        if (Request.HasFormContentType)
        {
            var form =
                await Request.ReadFormAsync(
                    cancellationToken);

            Console.WriteLine("FORM VALUES");
            Console.WriteLine("--------------------------------");

            foreach (var item in form)
            {
                Console.WriteLine(
                    $"{item.Key} = {item.Value}");
            }

            Console.WriteLine();

            if (form.TryGetValue("cmd", out var cmd))
            {
                Console.WriteLine();
                Console.WriteLine("COMMAND DETECTED");
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"CMD = {cmd}");
                if (form.TryGetValue("count", out var count))
                {
                    Console.WriteLine($"CACHE COUNT = {count}");
                }

                if (form.TryGetValue("data", out var data))
                {
                    Console.WriteLine(cmd == "cache" ? "CACHE DATA" : "REQUEST DATA");
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine(data);
                }
                Console.WriteLine();
            }
        }

        Request.Body.Position = 0;

        using var reader = new StreamReader(
            Request.Body,
            Encoding.UTF8,
            leaveOpen: true);

        var rawBody =
            await reader.ReadToEndAsync(
                cancellationToken);

        Request.Body.Position = 0;

        sb.AppendLine("RAW BODY");
        sb.AppendLine("--------------------------------");
        sb.AppendLine(rawBody);
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(ip))
        {
            var device =
                await _deviceRepository
                    .GetByIpAddressAsync(
                        ip,
                        cancellationToken);

            if (device is null)
            {
                sb.AppendLine(
                    $"AUTO DISCOVERY -> Creating {ip}");

                await _deviceRepository.CreateAsync(
                    new Device
                    {
                        Id = Guid.NewGuid(),

                        StoreId = Guid.Parse(
                            DemoStoreId),

                        SerialNumber =
                            $"AUTO-{ip}",

                        Name =
                            $"HP015-{ip}",

                        DeviceType =
                            "HP015",

                        ApiKey =
                            Guid.NewGuid().ToString(),

                        IpAddress = ip,

                        IsOnline = true,
                        IsActive = true,
                        IsDeleted = false,

                        LastTotalIn = 0,
                        LastTotalOut = 0,

                        CreatedAtUtc =
                            DateTime.UtcNow,

                        LastSeenAtUtc =
                            DateTime.UtcNow
                    },
                    cancellationToken);

                sb.AppendLine(
                    "AUTO DISCOVERY -> CREATED");
            }
            else
            {
                await _deviceRepository
                    .UpdateHeartbeatAsync(
                        device.Id,
                        DateTime.UtcNow,
                        true,
                        cancellationToken);

                sb.AppendLine(
                    $"HEARTBEAT UPDATED -> {device.Name}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("================================");
        sb.AppendLine("END REQUEST");
        sb.AppendLine("================================");
        sb.AppendLine();

        var log = sb.ToString();

        Console.WriteLine(log);

        try
        {
            var folder =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "logs");

            Directory.CreateDirectory(folder);

            var file =
                Path.Combine(
                    folder,
                    $"hp015-{DateTime.UtcNow:yyyyMMdd}.log");

            await System.IO.File.AppendAllTextAsync(
                file,
                log,
                cancellationToken);
        }
        catch
        {
        }

        // IMPORTANTE
        // Respuesta ultra simple para firmware embebido

        string responseBody = "result=";

        try
        {
            if (Request.HasFormContentType)
            {
                var form =
                    await Request.ReadFormAsync(
                        cancellationToken);

                var dictionary =
                    form.ToDictionary(
                        x => x.Key,
                        x => x.Value.ToString());

                using var client = new HttpClient();

                var response =
                    await client.PostAsync(
                        "http://13.94.116.51:3099/",
                        new FormUrlEncodedContent(
                            dictionary),
                        cancellationToken);

                responseBody =
                    await response.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                Console.WriteLine();
                Console.WriteLine("OFFICIAL SERVER RESPONSE");
                Console.WriteLine("--------------------------------");
                Console.WriteLine(responseBody);
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("PROXY ERROR");
            Console.WriteLine("--------------------------------");
            Console.WriteLine(ex);
            Console.WriteLine();
        }

        return Content(
            responseBody,
            "text/plain");
    }
}