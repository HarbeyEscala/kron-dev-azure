using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Mappings;
using Kron.Counting.Domain.Entities;
using Kron.Counting.Shared.Exceptions;

namespace Kron.Counting.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher)
    {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<UserDto>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var users =
            await _userRepository.GetByTenantIdAsync(
                tenantId,
                cancellationToken);

        return users.Select(x => x.ToDto());
    }

    public async Task<UserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userRepository.GetByIdAsync(
                id,
                cancellationToken);

        return user?.ToDto();
    }

    public async Task<Guid> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tenant =
            await _tenantRepository.GetByIdAsync(
                request.TenantId,
                cancellationToken);

        if (tenant is null)
            throw new KeyNotFoundException("Tenant not found.");

        var existing =
            await _userRepository.GetByEmailAsync(
                request.TenantId,
                request.Email,
                cancellationToken);

        if (existing is not null)
            throw new ConflictException(
                $"User with email '{request.Email}' already exists.");

        var passwordHash =
            _passwordHasher.HashPassword(request.Password);

        var entity = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = request.Email.Trim().ToLowerInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = passwordHash,
            Role = request.Role.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _userRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("User not found.");

        existing.FirstName = request.FirstName.Trim();
        existing.LastName = request.LastName.Trim();
        existing.Role = request.Role.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing =
            await _userRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (existing is null)
            throw new KeyNotFoundException("User not found.");

        await _userRepository.SoftDeleteAsync(id, cancellationToken);
    }
}