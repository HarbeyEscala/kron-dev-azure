using Kron.Counting.API.Helpers;
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
    private static readonly Hpc015sGetsettingSessionManager _getsettingSessionManager = new();
    private readonly IDevicePayloadRepository _devicePayloadRepository;
    private readonly IDevicePayloadProcessor _devicePayloadProcessor;
    private readonly TelemetrySettings _telemetrySettings;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(
        IDeviceRepository deviceRepository,
        IDevicePayloadRepository devicePayloadRepository,
        IDevicePayloadProcessor devicePayloadProcessor,
        IOptions<TelemetrySettings> telemetrySettings,
        ILogger<TelemetryController> logger)
    {
        _deviceRepository = deviceRepository;
        _devicePayloadRepository = devicePayloadRepository;
        _devicePayloadProcessor = devicePayloadProcessor;
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

        var ip = ClientIpHelper.Resolve(HttpContext);

        string? serialNumber = null;

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

        IFormCollection? form = null;

        if (Request.HasFormContentType)
        {
            form =
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

                if (cmd == "getsetting" &&
                    form.TryGetValue("data", out var getsettingData))
                {
                    serialNumber =
                        TryParseSerialFromGetsetting(
                            getsettingData.ToString(),
                            out var getSettingParsed);

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

                if (cmd == "cache")
                {
                    var device =
                        await ResolveDeviceForCacheAsync(
                            serialNumber,
                            ip,
                            cancellationToken);

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
                    else
                    {
                        _logger.LogWarning(
                            "Cache payload received but device could not be resolved. SN={SerialNumber}, IP={IpAddress}",
                            serialNumber ?? "(none)",
                            ip ?? "(none)");
                    }
                }

                if (form.TryGetValue("count", out var count))
                {
                    Console.WriteLine($"CACHE COUNT = {count}");
                }

                if (form.TryGetValue("data", out var data) &&
                    cmd != "getsetting")
                {
                    Console.WriteLine("CACHE DATA");
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine(data);
                }

                Console.WriteLine();
            }
        }

        await RegisterOrRefreshDeviceBySerialAsync(
            serialNumber,
            ip,
            sb,
            cancellationToken);

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
            if (form is not null)
            {
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

    private async Task<Device?> ResolveDeviceForCacheAsync(
        string? serialNumber,
        string? ip,
        CancellationToken cancellationToken)
    {
        var normalizedSerial = NormalizeSerial(serialNumber);

        if (normalizedSerial is not null)
        {
            var deviceBySerial =
                await _deviceRepository.GetBySerialNumberAsync(
                    normalizedSerial,
                    cancellationToken);

            if (deviceBySerial is not null)
            {
                _logger.LogDebug(
                    "Cache device resolved by SerialNumber {SerialNumber}",
                    normalizedSerial);

                return deviceBySerial;
            }
        }

        if (!string.IsNullOrWhiteSpace(ip))
        {
            if (_telemetrySettings.AllowLegacyIpIdentification)
            {
                var deviceByIp =
                    await _deviceRepository.GetByIpAddressAsync(
                        ip,
                        cancellationToken);

                if (deviceByIp is not null)
                {
                    _logger.LogDebug(
                        "Cache device resolved by IP {IpAddress}",
                        ip);

                    return deviceByIp;
                }
            }
            else
            {
                _logger.LogWarning(
                    "Legacy IP identification disabled.");
            }
        }

        return null;
    }

    private static string? TryParseSerialFromGetsetting(
        string? dataHex,
        out Hpc015sGetsettingRequest? parsed)
    {
        parsed =
            Hpc015sGetsettingRequest.Parse(
                dataHex ?? string.Empty);

        return NormalizeSerial(parsed?.Sn);
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

    private async Task RegisterOrRefreshDeviceBySerialAsync(
        string? serialNumber,
        string? ip,
        StringBuilder sb,
        CancellationToken cancellationToken)
    {
        var normalizedSerial = NormalizeSerial(serialNumber);

        if (normalizedSerial is null)
        {
            sb.AppendLine("AUTO DISCOVERY -> skipped (serial required)");
            _logger.LogDebug(
                "Auto discovery skipped because serial number was not present.");
            return;
        }

        var device =
            await _deviceRepository.GetBySerialNumberAsync(
                normalizedSerial,
                cancellationToken);

        if (device is not null)
        {
            await _deviceRepository.UpdateConnectionAsync(
                device.Id,
                DateTime.UtcNow,
                true,
                ip,
                cancellationToken);

            sb.AppendLine(
                $"HEARTBEAT UPDATED -> {device.Name} (SN={normalizedSerial}, IP={ip})");
            return;
        }

        if (!_telemetrySettings.AllowAutoDiscovery)
        {
            _logger.LogWarning(
                "Auto discovery disabled. Device not found for serial {SerialNumber}.",
                normalizedSerial);
            sb.AppendLine(
                $"AUTO DISCOVERY -> disabled for SN {normalizedSerial}");
            return;
        }

        sb.AppendLine($"AUTO DISCOVERY -> SN {normalizedSerial}");
        sb.AppendLine($"AUTO DISCOVERY -> Creating (IP={ip})");

        await _deviceRepository.CreateAsync(
            new Device
            {
                Id = Guid.NewGuid(),
                StoreId = null,
                SerialNumber = normalizedSerial,
                Name = $"HP015-{normalizedSerial}",
                ProvisioningStatus = "Pending",
                DeviceType = "HP015",
                ApiKey = Guid.NewGuid().ToString(),
                IpAddress = ip,
                IsOnline = true,
                IsActive = true,
                IsDeleted = false,
                LastTotalIn = 0,
                LastTotalOut = 0,
                CreatedAtUtc = DateTime.UtcNow,
                LastSeenAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        sb.AppendLine("AUTO DISCOVERY -> CREATED");
    }

    private static string? NormalizeSerial(string? serialNumber) =>
        string.IsNullOrWhiteSpace(serialNumber)
            ? null
            : serialNumber.Trim().ToUpperInvariant();
}