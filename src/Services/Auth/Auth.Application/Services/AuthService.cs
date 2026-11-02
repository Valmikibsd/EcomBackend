using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var identifier = !string.IsNullOrWhiteSpace(dto.Username) ? dto.Username.Trim() : dto.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(dto.Password))
        {
            throw new ArgumentException("Username/Email and Password are required.");
        }

        // Validate credentials using stored procedure USP_User_Login or database query
        var user = await _userRepository.ValidateUserLoginProcAsync(identifier, dto.Password);
        if (user == null)
        {
            // Fallback check by username/email
            user = await _userRepository.GetByUsernameOrEmailAsync(identifier);
            if (user == null || user.Password != dto.Password)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateRefreshTokenProcAsync(user.Id, refreshToken, expiry);

        return new AuthResponse
        {
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddHours(1).ToString("o"),
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username and Password are required.");
        }

        var result = await _userRepository.RegisterUserProcAsync(
            dto.Username.Trim(),
            dto.Email?.Trim() ?? $"{dto.Username}@example.com",
            dto.Password,
            string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role
        );

        if (result == -1)
        {
            throw new InvalidOperationException("User with this username or email already exists.");
        }

        return result > 0;
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var user = await _userRepository.GetByRefreshTokenProcAsync(dto.RefreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        await _userRepository.UpdateRefreshTokenProcAsync(user.Id, newRefreshToken, expiry);

        return new AuthResponse
        {
            Token = newAccessToken,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddHours(1).ToString("o"),
            Username = user.Username,
            Role = user.Role
        };
    }
}
