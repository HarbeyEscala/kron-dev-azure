using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;
using Kron.Counting.Domain.Entities;

namespace Kron.Counting.Application.Services;

public sealed class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IMeasurementPointRepository _measurementPointRepository;
    private readonly IDeviceAssignmentRepository _deviceAssignmentRepository;
    private readonly IDevicePayloadRepository _devicePayloadRepository;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IStoreRepository storeRepository,
        IMeasurementPointRepository measurementPointRepository,
        IDeviceAssignmentRepository deviceAssignmentRepository,
        IDevicePayloadRepository devicePayloadRepository)
    {
        _deviceRepository = deviceRepository;
        _storeRepository = storeRepository;
        _measurementPointRepository = measurementPointRepository;
        _deviceAssignmentRepository = deviceAssignmentRepository;
        _devicePayloadRepository = devicePayloadRepository;
    }

    public async Task<IEnumerable<DeviceDto>> GetByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var devices =
            await _deviceRepository.GetByStoreIdAsync(
                storeId,
                cancellationToken);

        return devices.Select(x => x.ToDto());
    }

    public async Task<DeviceDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                id,
                cancellationToken);

        return device?.ToDto();
    }

    public async Task<IEnumerable<DeviceDto>> GetPendingAsync(
    CancellationToken cancellationToken = default)
    {
        var devices =
            await _deviceRepository.GetPendingAsync(
                cancellationToken);

        return devices.Select(x => x.ToDto());
    }

    public async Task<Guid> CreateAsync(
        CreateDeviceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var store =
            await _storeRepository.GetByIdAsync(
                request.StoreId,
                cancellationToken);

        if (store is null)
            throw new KeyNotFoundException("Store not found.");

        var existing =
            await _deviceRepository.GetBySerialNumberAsync(
                request.StoreId,
                request.SerialNumber,
                cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException(
                $"Device with serial '{request.SerialNumber}' already exists.");

        var entity = new Device
        {
            Id = Guid.NewGuid(),
            StoreId = request.StoreId,
            SerialNumber = request.SerialNumber.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            DeviceType = request.DeviceType.Trim(),
            ApiKey = Guid.NewGuid().ToString(),
            FirmwareVersion = request.FirmwareVersion?.Trim(),
            IsOnline = false,
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _deviceRepository.CreateAsync(
            entity,
            cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateDeviceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _deviceRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Device not found.");

        existing.Name = request.Name.Trim();
        existing.DeviceType = request.DeviceType.Trim();
        existing.FirmwareVersion = request.FirmwareVersion?.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _deviceRepository.UpdateAsync(
            existing,
            cancellationToken);
    }

    public async Task ProvisionAsync(
        Guid deviceId,
        Guid tenantId,
        ProvisionDeviceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var device =
            await _deviceRepository.GetByIdAsync(
                deviceId,
                cancellationToken);

        if (device is null)
            throw new KeyNotFoundException(
                "Device not found.");

        var measurementPoint =
            await _measurementPointRepository.GetByIdAsync(
                request.MeasurementPointId);

        if (measurementPoint is null)
            throw new KeyNotFoundException("Measurement point not found.");

        await _deviceRepository.ProvisionAsync(
            deviceId,
            tenantId,
            measurementPoint.StoreId,
            request.Name,
            cancellationToken);

        var activeAssignment =
            await _deviceAssignmentRepository.GetActiveAssignmentAsync(deviceId);

        if (activeAssignment is not null)
        {
            await _deviceAssignmentRepository.CloseAssignmentAsync(
                activeAssignment.Id);
        }

        var assignment = new DeviceAssignment
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            MeasurementPointId = request.MeasurementPointId,
            AssignedAtUtc = DateTime.UtcNow,
            UnassignedAtUtc = null,
            BaselineTotalIn = device.LastTotalIn,
            BaselineTotalOut = device.LastTotalOut,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _deviceAssignmentRepository.CreateAsync(assignment);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _deviceRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Device not found.");

        await _deviceRepository.SoftDeleteAsync(
            id,
            cancellationToken);
    }

    public async Task<DeviceHealthSummaryDto>
        GetHealthSummaryAsync(
            CancellationToken cancellationToken = default)
    {
        var devices =
            (await _deviceRepository.GetAllAsync(
                cancellationToken))
            .ToList();

        return new DeviceHealthSummaryDto
        {
            TotalDevices = devices.Count,

            OnlineDevices =
                devices.Count(x => x.IsOnline),

            OfflineDevices =
                devices.Count(x => !x.IsOnline),

            ActiveDevices =
                devices.Count(x => x.IsActive),

            InactiveDevices =
                devices.Count(x => !x.IsActive)
        };
    }

    public async Task<IEnumerable<OfflineDeviceDto>> GetOfflineDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var devices =
            (await _deviceRepository.GetAllAsync(
                cancellationToken))
            .Where(x => !x.IsOnline)
            .OrderBy(x => x.LastSeenAtUtc ?? DateTime.MinValue)
            .ToList();

        return devices.Select(x => new OfflineDeviceDto
        {
            DeviceId = x.Id,
            Name = x.Name,
            SerialNumber = x.SerialNumber,
            IpAddress = x.IpAddress,
            FirmwareVersion = x.FirmwareVersion,
            LastSeenAtUtc = x.LastSeenAtUtc,
            MinutesOffline = x.LastSeenAtUtc.HasValue
                ? Math.Max(
                    0,
                    (int)(utcNow - x.LastSeenAtUtc.Value).TotalMinutes)
                : 0
        });
    }

    public async Task<IEnumerable<SilentDeviceDto>>
        GetSilentDevicesAsync(
            CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var devices =
            (await _deviceRepository.GetAllAsync(
                cancellationToken))
            .Where(x => x.IsOnline)
            .ToList();

        var result =
            new List<SilentDeviceDto>();

        foreach (var device in devices)
        {
            var lastPayloadUtc =
                await _devicePayloadRepository
                    .GetLastPayloadUtcAsync(
                        device.Id,
                        cancellationToken);

            if (!lastPayloadUtc.HasValue)
                continue;

            var minutesSilent =
                (int)(utcNow - lastPayloadUtc.Value)
                .TotalMinutes;

            if (minutesSilent < 60)
                continue;

            result.Add(
                new SilentDeviceDto
                {
                    DeviceId = device.Id,
                    Name = device.Name,
                    SerialNumber = device.SerialNumber,
                    LastPayloadUtc = lastPayloadUtc,
                    MinutesSilent = minutesSilent
                });
        }

        return result
            .OrderByDescending(x => x.MinutesSilent);
    }
}
