using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Helpers;
using Kron.Counting.Shared.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
using Kron.Counting.Domain.Constants;

namespace Kron.Counting.API.Controllers;

[ApiController]
[Route("api/v1/internal/device-readings")]
public sealed class TelemetryController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceReadingRepository _deviceReadingRepository;
    private static readonly Hpc015sGetsettingSessionManager _getsettingSessionManager = new();
    private readonly IDevicePayloadRepository _devicePayloadRepository;
    private readonly IDevicePayloadProcessor _devicePayloadProcessor;
    private readonly ITelemetryDeviceResolver _telemetryDeviceResolver;
    private readonly TelemetrySettings _telemetrySettings;
    private readonly ILogger<TelemetryController> _logger;

    private const string DemoStoreId =
        "75380B27-528D-48F8-85A3-136AFE523F08";

    public TelemetryController(
        IDeviceRepository deviceRepository,
        IDeviceReadingRepository deviceReadingRepository,
        IDevicePayloadRepository devicePayloadRepository,
        IDevicePayloadProcessor devicePayloadProcessor,
        ITelemetryDeviceResolver telemetryDeviceResolver,
        IOptions<TelemetrySettings> telemetrySettings,
        ILogger<TelemetryController> logger)
    {
        _deviceRepository = deviceRepository;
        _deviceReadingRepository = deviceReadingRepository;
        _devicePayloadRepository = devicePayloadRepository;
        _devicePayloadProcessor = devicePayloadProcessor;
        _telemetryDeviceResolver = telemetryDeviceResolver;
        _telemetrySettings = telemetrySettings.Value;
        _logger = logger;
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

        string? serialNumber = null;

        var apiKey = ReadApiKey();
        Device? resolvedDevice = null;
        var resolvedByApiKey = false;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            resolvedDevice =
                await _telemetryDeviceResolver.ResolveByApiKeyAsync(
                    apiKey,
                    cancellationToken);

            if (resolvedDevice is not null)
            {
                resolvedByApiKey = true;

                _logger.LogInformation(
                    "Telemetry device resolved by ApiKey");

                await _deviceRepository.UpdateHeartbeatAsync(
                    resolvedDevice.Id,
                    DateTime.UtcNow,
                    true,
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Telemetry ApiKey present but did not resolve to a device");
            }
        }

        if (!_telemetrySettings.AllowLegacyWithoutApiKey &&
            !resolvedByApiKey)
        {
            return Content("result=", "text/plain");
        }

        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("================================");
        sb.AppendLine("HP015 REQUEST DETECTED");
        sb.AppendLine("================================");
        sb.AppendLine($"Timestamp UTC : {now:O}");
        sb.AppendLine($"Remote IP     : {ip}");
        sb.AppendLine($"Method        : {Request.Method}");
        sb.AppendLine($"Path          : {Request.Path}");
        sb.AppendLine($"QueryString   : {RedactSensitiveValues(Request.QueryString.ToString())}");
        sb.AppendLine($"ContentType   : {Request.ContentType}");
        sb.AppendLine();

        sb.AppendLine("HEADERS");
        sb.AppendLine("--------------------------------");

        foreach (var header in Request.Headers)
        {
            sb.AppendLine(
                RedactSensitiveValues(
                    $"{header.Key} = {header.Value}"));
        }

        sb.AppendLine();

        if (Request.Query.Any())
        {
            sb.AppendLine("QUERY VALUES");
            sb.AppendLine("--------------------------------");

            foreach (var item in Request.Query)
            {
                sb.AppendLine(
                    RedactSensitiveValues(
                        $"{item.Key} = {item.Value}"));
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
                    Device? device = null;

                    if (resolvedByApiKey)
                    {
                        device = resolvedDevice;
                    }
                    else if (!string.IsNullOrWhiteSpace(ip))
                    {
                        if (_telemetrySettings.AllowLegacyIpIdentification)
                        {
                            device =
                                await _deviceRepository
                                    .GetByIpAddressAsync(
                                        ip,
                                        cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Legacy IP identification disabled.");
                        }
                    }

                    if (device is not null)
                    {
                        foreach (var dataRecord in form["data"])
                        {
                            try
                            {
                                var payload = new DevicePayload
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceId = device.Id,
                                    PayloadType = "cache",
                                    RawHex = dataRecord!,
                                    Status = PayloadStatuses.Received,
                                    ReceivedAtUtc = DateTime.UtcNow
                                };

                                await _devicePayloadRepository.InsertAsync(payload);

                                await _devicePayloadProcessor.ProcessAsync(
                                    payload,
                                    cancellationToken);
                                //var parsed =
                                //    Hp015Parser.Parse(
                                //        dataRecord!);

                                //Console.WriteLine();
                                //Console.WriteLine("PARSED RECORD");
                                //Console.WriteLine("--------------------------------");
                                //Console.WriteLine($"Timestamp : {parsed.TimestampUtc}");
                                //Console.WriteLine($"IN        : {parsed.PeopleIn}");
                                //Console.WriteLine($"OUT       : {parsed.PeopleOut}");

                                //var reading = new DeviceReading
                                //{
                                //    DeviceId = device.Id,

                                //    ReadingTimestampUtc =
                                //        parsed.TimestampUtc,

                                //    PeopleIn =
                                //        parsed.PeopleIn,

                                //    PeopleOut =
                                //        parsed.PeopleOut,

                                //    Occupancy =
                                //        Math.Max(
                                //            0,
                                //            parsed.PeopleIn -
                                //            parsed.PeopleOut),

                                //    RawPayloadJson =
                                //        parsed.RawData,

                                //    CreatedAtUtc =
                                //        DateTime.UtcNow
                                //};

                                //var exists =
                                //    await _deviceReadingRepository
                                //        .ExistsAsync(
                                //            reading.DeviceId,
                                //            reading.ReadingTimestampUtc,
                                //            reading.PeopleIn,
                                //            reading.PeopleOut);

                                //if (exists)
                                //{
                                //    await _devicePayloadRepository.UpdateStatusAsync(
                                //        payload.Id,
                                //        PayloadStatuses.Duplicate);

                                //    Console.WriteLine();
                                //    Console.WriteLine("DUPLICATE READING");
                                //    Console.WriteLine("--------------------------------");
                                //    Console.WriteLine(
                                //        $"{reading.ReadingTimestampUtc:yyyy-MM-dd HH:mm:ss} | " +
                                //        $"IN={reading.PeopleIn} | " +
                                //        $"OUT={reading.PeopleOut}");

                                //    continue;
                                //}

                                //try
                                //{
                                //    await _deviceReadingRepository
                                //        .CreateAsync(
                                //            reading,
                                //            cancellationToken);

                                //    await _devicePayloadRepository.UpdateStatusAsync(
                                //        payload.Id,
                                //        PayloadStatuses.Saved);

                                //    Console.WriteLine();
                                //    Console.WriteLine("DEVICE READING SAVED");
                                //    Console.WriteLine("--------------------------------");
                                //    Console.WriteLine(
                                //        $"{reading.ReadingTimestampUtc:yyyy-MM-dd HH:mm:ss} | " +
                                //        $"IN={reading.PeopleIn} | " +
                                //        $"OUT={reading.PeopleOut}");
                                //}
                                //catch (Exception ex)
                                //{
                                //    await _devicePayloadRepository.UpdateStatusAsync(
                                //        payload.Id,
                                //        PayloadStatuses.Failed,
                                //        ex.Message);

                                //    Console.WriteLine();
                                //    Console.WriteLine("DEVICE READING ERROR");
                                //    Console.WriteLine("--------------------------------");
                                //    Console.WriteLine(ex.Message);
                                //}
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
                    Console.WriteLine(
                        cmd == "cache"
                            ? "CACHE DATA"
                            : "REQUEST DATA");

                    Console.WriteLine("--------------------------------");
                    Console.WriteLine(data);

                    if (cmd == "getsetting")
                    {
                        var getSettingParsed =
                            Hpc015sGetsettingRequest.Parse(
                                data.ToString());

                        serialNumber =
                            getSettingParsed?.Sn;

                        if (getSettingParsed is not null)
                        {
                            Console.WriteLine();
                            Console.WriteLine("GETSETTING PARSED");
                            Console.WriteLine("--------------------------------");
                            Console.WriteLine(
                                $"RecordingCycle = {getSettingParsed.RecordingCycle}");

                            Console.WriteLine(
                                $"UploadCycle    = {getSettingParsed.UploadCycle}");
                            Console.WriteLine(
                                $"SN             = {getSettingParsed.Sn}");
                            Console.WriteLine(
                                $"MAC            = {BitConverter.ToString(getSettingParsed.MacRaw ?? [])}");
                        }
                    }
                }

                Console.WriteLine();
            }
        }


        if (!resolvedByApiKey && !string.IsNullOrWhiteSpace(ip))
        {
            Device? device = null;

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                device =
                    await _deviceRepository
                        .GetBySerialNumberAsync(
                            serialNumber,
                            cancellationToken);
            }

            if (device is null)
            {
                if (_telemetrySettings.AllowLegacyIpIdentification)
                {
                    device =
                        await _deviceRepository
                            .GetByIpAddressAsync(
                                ip,
                                cancellationToken);
                }
                else
                {
                    _logger.LogWarning(
                        "Legacy IP identification disabled.");
                }
            }

            if (device is null)
            {
                if (_telemetrySettings.AllowAutoDiscovery)
                {
                    sb.AppendLine(
                        $"AUTO DISCOVERY -> SN {serialNumber}");

                    sb.AppendLine(
                        $"AUTO DISCOVERY -> Creating {ip}");

                    await _deviceRepository.CreateAsync(
                        new Device
                        {
                            Id = Guid.NewGuid(),

                            StoreId = null,

                            SerialNumber =
                                serialNumber ?? $"AUTO-{ip}",

                            Name =
                                $"HP015-{serialNumber ?? ip}",

                            ProvisioningStatus = "Pending",

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
                    _logger.LogWarning(
                        "Auto discovery disabled. Device not found.");
                }
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

                if (form.TryGetValue("cmd", out var cmdCache) &&
                    cmdCache == "cache" &&
                    form.TryGetValue("flag", out var cacheFlag))
                                {
                                    var cacheResult =
                                        Hpc015sCacheProtocol
                                            .BuildCacheSuccessResponseHex(
                                                cacheFlag.ToString());

                                    responseBody =
                                        $"result={cacheResult}";

                                    Console.WriteLine();
                                    Console.WriteLine("CACHE ACK SENT");
                                    Console.WriteLine("--------------------------------");
                                    Console.WriteLine(responseBody);
                                    Console.WriteLine();
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

        Console.WriteLine();
        Console.WriteLine("RESPONSE SENT");
        Console.WriteLine("--------------------------------");
        Console.WriteLine(responseBody);
        Console.WriteLine();

        return Content(
            responseBody,
            "text/plain");
    }

    private static string RedactSensitiveValues(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = Regex.Replace(
            input,
            @"(?i)(apiKey\s*=\s*)([^\s&]*)",
            "$1***");

        result = Regex.Replace(
            result,
            @"(?i)(X-Device-Api-Key\s*[:=]\s*)([^\s&,]*)",
            "$1***");

        return result;
    }

    private string? ReadApiKey()
    {
        if (Request.Headers.TryGetValue(
                "X-Device-Api-Key",
                out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        if (Request.Query.TryGetValue(
                "apiKey",
                out var queryValue) &&
            !string.IsNullOrWhiteSpace(queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}