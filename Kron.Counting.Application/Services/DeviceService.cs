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

    public DeviceService(
        IDeviceRepository deviceRepository,
        IStoreRepository storeRepository)
    {
        _deviceRepository = deviceRepository;
        _storeRepository = storeRepository;
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

        var store =
            await _storeRepository.GetByIdAsync(
                request.StoreId,
                cancellationToken);

        if (store is null)
            throw new KeyNotFoundException(
                "Store not found.");

        await _deviceRepository.ProvisionAsync(
            deviceId,
            tenantId,
            request.StoreId,
            request.Name,
            cancellationToken);
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
}
