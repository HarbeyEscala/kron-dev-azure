using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Kron.Counting.Shared.Helpers;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/internal/device-readings")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceReadingRepository _deviceReadingRepository;
    private static readonly Hpc015sGetsettingSessionManager _getsettingSessionManager = new();

    private const string DemoStoreId =
        "0341DF21-4D90-4E3D-B054-BCD9EC47573D";

    public TelemetryController(
        IDeviceRepository deviceRepository,
        IDeviceReadingRepository deviceReadingRepository)
    {
        _deviceRepository = deviceRepository;
        _deviceReadingRepository = deviceReadingRepository;
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
                if (cmd == "cache")
                {
                    var device =
                        await _deviceRepository
                            .GetByIpAddressAsync(
                                ip!,
                                cancellationToken);

                    if (device is not null)
                    {
                        foreach (var dataRecord in form["data"])
                        {
                            try
                            {
                                var parsed =
                                    Hp015Parser.Parse(
                                        dataRecord!);

                                Console.WriteLine();
                                Console.WriteLine("PARSED RECORD");
                                Console.WriteLine("--------------------------------");
                                Console.WriteLine(
                                    $"Timestamp : {parsed.TimestampUtc}");

                                Console.WriteLine(
                                    $"IN        : {parsed.PeopleIn}");

                                Console.WriteLine(
                                    $"OUT       : {parsed.PeopleOut}");

                                await _deviceReadingRepository
                                    .CreateAsync(
                                        new DeviceReading
                                        {
                                            DeviceId =
                                                device.Id,

                                            ReadingTimestampUtc =
                                                parsed.TimestampUtc,

                                            PeopleIn =
                                                parsed.PeopleIn,

                                            PeopleOut =
                                                parsed.PeopleOut,

                                            Occupancy =
                                                Math.Max(
                                                    0,
                                                    parsed.PeopleIn -
                                                    parsed.PeopleOut),

                                            RawPayloadJson =
                                                parsed.RawData,

                                            CreatedAtUtc =
                                                DateTime.UtcNow
                                        },
                                        cancellationToken);

                                Console.WriteLine(
                                    "DEVICE READING SAVED");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine();
                                Console.WriteLine("PARSE ERROR");
                                Console.WriteLine("--------------------------------");
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
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

                if (form.TryGetValue("cmd", out var cmd) &&
                    cmd == "getsetting" &&
                    form.TryGetValue("flag", out var flag) &&
                    form.TryGetValue("data", out var data))
                {
                    var request =
                        Hpc015sGetsettingRequest.Parse(
                            data.ToString());

                    if (request is not null && request.Ok)
                    {
                        var shouldConfirm =
                            _getsettingSessionManager.ShouldConfirm(
                                request.Sn,
                                request.RawDataHex);

                        var response =
                            new Hpc015sGetsettingResponse(
                                flag.ToString(),
                                request.SnRaw)
                            {
                                RespondingType =
                                    shouldConfirm
                                        ? (byte)0x05
                                        : (byte)0x04,

                                CommandType = request.CommandType,
                                Speed = request.Speed,

                                RecordingCycle = 1,
                                UploadCycle = 1,

                                FixedTime = 0,
                                Model = 0,

                                DisableType =
                                    request.DisableType,

                                Year = request.Year,
                                Month = request.Month,
                                Day = request.Day,

                                Hour = request.Hour,
                                Minute = request.Minute,
                                Second = request.Second,

                                Week = request.Week,

                                OpenHour = request.OpenHour,
                                OpenMinute = request.OpenMinute,

                                CloseHour = request.CloseHour,
                                CloseMinute = request.CloseMinute
                            };

                        var resultHex =
                            response.Build();

                        responseBody =
                            $"result={resultHex}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("GETSETTING ERROR");
            Console.WriteLine("--------------------------------");
            Console.WriteLine(ex);
            Console.WriteLine();
        }

        return Content(
            responseBody,
            "text/plain");
        }
    }