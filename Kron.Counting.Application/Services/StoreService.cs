using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Exceptions;

namespace Kron.Counting.Application.Services;

public sealed class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMeasurementPointRepository _measurementPointRepository;

    public StoreService(
        IStoreRepository storeRepository,
        ITenantRepository tenantRepository,
        IMeasurementPointRepository measurementPointRepository)
    {
        _storeRepository = storeRepository;
        _tenantRepository = tenantRepository;
        _measurementPointRepository = measurementPointRepository;
    }

    public async Task<IEnumerable<StoreDto>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var stores =
            await _storeRepository.GetByTenantIdAsync(
                tenantId,
                cancellationToken);

        return stores.Select(x => x.ToDto());
    }

    public async Task<StoreDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var store =
            await _storeRepository.GetByIdAsync(
                id,
                cancellationToken);

        return store?.ToDto();
    }

    public async Task<Guid> CreateAsync(
        CreateStoreRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tenant =
            await _tenantRepository.GetByIdAsync(
                request.TenantId,
                cancellationToken);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        var existing =
            await _storeRepository.GetByCodeAsync(
                request.TenantId,
                request.Code,
                cancellationToken);

        if (existing is not null)
            throw new ConflictException(
                $"Store with code '{request.Code}' already exists.");

        var entity = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Country = request.Country,
            State = request.State,
            City = request.City,
            PostalCode = request.PostalCode,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TimeZone = request.TimeZone,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Capacity = request.Capacity,
            StoreType = request.StoreType?.Trim(),
            Region = request.Region?.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _storeRepository.CreateAsync(entity, cancellationToken);

        var measurementPoint =
            new MeasurementPoint
            {
                Id = Guid.NewGuid(),
                StoreId = entity.Id,
                Name = "Main Entrance",
                Description =
                    "Auto-generated default measurement point",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

        await _measurementPointRepository.CreateAsync(
            measurementPoint,
            cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateStoreRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _storeRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Store not found.");

        existing.Name = request.Name.Trim();
        existing.Description = request.Description?.Trim();
        existing.StoreType = request.StoreType?.Trim();
        existing.Region = request.Region?.Trim();
        existing.Country = request.Country;

        existing.State = request.State;
        existing.City = request.City;
        existing.PostalCode = request.PostalCode;
        existing.AddressLine1 = request.AddressLine1;
        existing.AddressLine2 = request.AddressLine2;
        existing.Latitude = request.Latitude;
        existing.Longitude = request.Longitude;
        existing.TimeZone = request.TimeZone;
        existing.ContactName = request.ContactName;
        existing.ContactEmail = request.ContactEmail;
        existing.ContactPhone = request.ContactPhone;
        existing.Capacity = request.Capacity;

        existing.IsActive = request.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _storeRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _storeRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Store not found.");

        await _storeRepository.SoftDeleteAsync(id, cancellationToken);
    }

    public async Task PatchAsync(
        Guid id,
        PatchStoreRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _storeRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException(
                "Store not found.");

        if (request.Name is not null)
            existing.Name = request.Name.Trim();

        if (request.Description is not null)
            existing.Description = request.Description.Trim();

        if (request.StoreType is not null)
            existing.StoreType = request.StoreType.Trim();

        if (request.Region is not null)
            existing.Region = request.Region.Trim();

        if (request.Country is not null)
            existing.Country = request.Country.Trim();

        if (request.State is not null)
            existing.State = request.State.Trim();

        if (request.City is not null)
            existing.City = request.City.Trim();

        if (request.PostalCode is not null)
            existing.PostalCode = request.PostalCode.Trim();

        if (request.AddressLine1 is not null)
            existing.AddressLine1 = request.AddressLine1.Trim();

        if (request.AddressLine2 is not null)
            existing.AddressLine2 = request.AddressLine2.Trim();

        if (request.Latitude.HasValue)
            existing.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            existing.Longitude = request.Longitude.Value;

        if (request.TimeZone is not null)
            existing.TimeZone = request.TimeZone.Trim();

        if (request.ContactName is not null)
            existing.ContactName = request.ContactName.Trim();

        if (request.ContactEmail is not null)
            existing.ContactEmail = request.ContactEmail.Trim();

        if (request.ContactPhone is not null)
            existing.ContactPhone = request.ContactPhone.Trim();

        if (request.Capacity.HasValue)
            existing.Capacity = request.Capacity.Value;

        if (request.IsActive.HasValue)
            existing.IsActive = request.IsActive.Value;

        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _storeRepository.UpdateAsync(
            existing,
            cancellationToken);
    }
}