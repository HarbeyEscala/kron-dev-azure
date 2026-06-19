using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Exceptions;

namespace Kron.Counting.Application.Services;

public sealed class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBrandRepository _brandRepository;

    public TenantService(
        ITenantRepository tenantRepository,
        IBrandRepository brandRepository)
    {
        _tenantRepository = tenantRepository;
        _brandRepository = brandRepository;
    }

    public async Task<IEnumerable<TenantDto>> GetByBrandIdAsync(
        Guid brandId,
        CancellationToken cancellationToken = default)
    {
        var tenants =
            await _tenantRepository.GetByBrandIdAsync(
                brandId,
                cancellationToken);

        return tenants.Select(x => x.ToDto());
    }

    public async Task<TenantDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenant =
            await _tenantRepository.GetByIdAsync(
                id,
                cancellationToken);

        return tenant?.ToDto();
    }

    public async Task<Guid> CreateAsync(
        CreateTenantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var brand =
            await _brandRepository.GetByIdAsync(
                request.BrandId,
                cancellationToken);

        if (brand is null)
            throw new KeyNotFoundException("Brand not found.");

        var existing =
            await _tenantRepository.GetByCodeAsync(
                request.BrandId,
                request.Code,
                cancellationToken);

        if (existing is not null)
            throw new ConflictException(
                $"Tenant with code '{request.Code}' already exists.");

        var entity = new Tenant
        {
            Id = Guid.NewGuid(),
            BrandId = request.BrandId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            TimeZone = request.TimeZone.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Locale = request.Locale.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _tenantRepository.CreateAsync(
            entity,
            cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateTenantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _tenantRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Tenant not found.");

        existing.Name = request.Name.Trim();
        existing.TimeZone = request.TimeZone.Trim();
        existing.Currency = request.Currency.Trim().ToUpperInvariant();
        existing.Locale = request.Locale.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _tenantRepository.UpdateAsync(
            existing,
            cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _tenantRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Tenant not found.");

        await _tenantRepository.SoftDeleteAsync(
            id,
            cancellationToken);
    }
}