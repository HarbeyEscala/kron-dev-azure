using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Exceptions;

namespace Kron.Counting.Application.Services;

public sealed class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;

    public BrandService(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<IEnumerable<BrandDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var brands = await _brandRepository.GetAllAsync(cancellationToken);

        return brands.Select(x => x.ToDto());
    }

    public async Task<BrandDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);

        return brand?.ToDto();
    }

    public async Task<Guid> CreateAsync(
        CreateBrandRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _brandRepository.GetByCodeAsync(
                request.Code,
                cancellationToken);

        if (existing is not null)
            throw new ConflictException(
                $"Brand with code '{request.Code}' already exists.");

        var entity = new Brand
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _brandRepository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateBrandRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _brandRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Brand not found.");

        existing.Name = request.Name.Trim();
        existing.Description = request.Description?.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _brandRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _brandRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("Brand not found.");

        await _brandRepository.SoftDeleteAsync(id, cancellationToken);
    }
}