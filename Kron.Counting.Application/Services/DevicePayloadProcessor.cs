using Kron.Counting.Application.Interfaces;
using Kron.Counting.Domain.Entities;
using System.Text;
using Kron.Counting.Shared.Helpers;
using Kron.Counting.Domain.Constants;

namespace Kron.Counting.Application.Services;

public sealed class DevicePayloadProcessor
    : IDevicePayloadProcessor
{
    private readonly IDeviceReadingRepository _deviceReadingRepository;
    private readonly IDevicePayloadRepository _devicePayloadRepository;

    public DevicePayloadProcessor(
        IDeviceReadingRepository deviceReadingRepository,
        IDevicePayloadRepository devicePayloadRepository)
    {
        _deviceReadingRepository = deviceReadingRepository;
        _devicePayloadRepository = devicePayloadRepository;
    }
    public async Task ProcessAsync(
    DevicePayload payload,
    CancellationToken cancellationToken = default)
    {
        var parsed =
            Hp015Parser.Parse(
                payload.RawHex);

        var reading = new DeviceReading
        {
            DeviceId = payload.DeviceId,

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
        };

        var exists =
            await _deviceReadingRepository
                .ExistsAsync(
                    reading.DeviceId,
                    reading.ReadingTimestampUtc,
                    reading.PeopleIn,
                    reading.PeopleOut);

            Console.WriteLine();
            Console.WriteLine("PAYLOAD PROCESSED");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"PayloadId = {payload.Id}");
            Console.WriteLine($"Status    = {PayloadStatuses.Saved}");

        if (exists)
        {
            await _devicePayloadRepository
                .UpdateStatusAsync(
                    payload.Id,
                    PayloadStatuses.Duplicate);
            Console.WriteLine();
            Console.WriteLine("DUPLICATE READING");
            Console.WriteLine("--------------------------------");
            Console.WriteLine(
                $"{reading.ReadingTimestampUtc:yyyy-MM-dd HH:mm:ss} | " +
                $"IN={reading.PeopleIn} | " +
                $"OUT={reading.PeopleOut}");

            return;
        }

        await _deviceReadingRepository
            .CreateAsync(
                reading,
                cancellationToken);

        await _devicePayloadRepository
            .UpdateStatusAsync(
                payload.Id,
                PayloadStatuses.Saved);
    }
}