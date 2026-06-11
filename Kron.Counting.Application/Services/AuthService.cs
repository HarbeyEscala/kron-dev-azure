using BCrypt.Net;
using Kron.Counting.Application.DTOs.Requests;
using Kron.Counting.Application.DTOs.Responses;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByGlobalEmailAsync(
            request.Email);

        Console.WriteLine($"LOGIN EMAIL: {request.Email}");
        Console.WriteLine(user is null ? "USER NOT FOUND" : $"USER FOUND: {user.Email}");
        if (user is null)
        {
            
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        Console.WriteLine($"PASSWORD VALID: {passwordValid}");
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        await _userRepository.UpdateLastLoginAsync(
            user.Id,
            DateTime.UtcNow);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresIn = 900,
            TokenType = "Bearer",

            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            Role = user.Role
        };
    }
}