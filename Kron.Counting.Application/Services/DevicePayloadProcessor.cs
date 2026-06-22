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
    private readonly IDeviceAssignmentResolver _deviceAssignmentResolver;
    private readonly IDeviceRepository _deviceRepository;

    public DevicePayloadProcessor(
        IDeviceReadingRepository deviceReadingRepository,
        IDevicePayloadRepository devicePayloadRepository,
        IDeviceAssignmentResolver deviceAssignmentResolver,
        IDeviceRepository deviceRepository)
    {
        _deviceReadingRepository = deviceReadingRepository;
        _devicePayloadRepository = devicePayloadRepository;
        _deviceAssignmentResolver = deviceAssignmentResolver;
        _deviceRepository = deviceRepository;
    }
    public async Task ProcessAsync(
        DevicePayload payload,
    CancellationToken cancellationToken = default)
    {
        var parsed =
            Hp015Parser.Parse(
                payload.RawHex);

        var assignmentContext =
            await _deviceAssignmentResolver.ResolveAsync(
                payload.DeviceId,
                parsed.TimestampUtc,
                cancellationToken);

        var reading = new DeviceReading
        {
            DeviceId = payload.DeviceId,

            DeviceAssignmentId =
                assignmentContext?.DeviceAssignmentId,

            MeasurementPointId =
                assignmentContext?.MeasurementPointId,

            StoreId =
                assignmentContext?.StoreId,

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

        try
        {
            await _deviceReadingRepository.CreateAsync(
                reading,
                cancellationToken);
        }
        catch(Exception ex)
{
            Console.WriteLine();
            Console.WriteLine("INSERT FAILED");
            Console.WriteLine("--------------------------------");
            Console.WriteLine(ex.Message);

            await _devicePayloadRepository.UpdateStatusAsync(
                payload.Id,
                PayloadStatuses.Duplicate);

            return;
        }

        await _deviceRepository.UpdateTelemetryStatusAsync(
            payload.DeviceId,
            DateTime.UtcNow,
            parsed.TimestampUtc,
            true,
            null,
            cancellationToken);

        await _devicePayloadRepository
            .UpdateStatusAsync(
                payload.Id,
                PayloadStatuses.Saved);
    }
}